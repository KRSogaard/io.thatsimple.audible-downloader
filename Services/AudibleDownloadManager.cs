using System.Text.Json;
using AudibleDownloader.DAL.Services;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Models;
using AudibleDownloader.Parser;
using AudibleDownloader.Queue;
using NLog;

namespace AudibleDownloader.Services;

public class AudibleDownloadManager {
    private readonly AuthorService authorService;
    private readonly BookService bookService;
    private readonly CategoryService categoryService;
    private readonly AudibleDataGetter dataGetter;
    private readonly DownloadQueue downloadQueue;
    private readonly DownloadService downloadService;
    private readonly Logger log = LogManager.GetCurrentClassLogger();
    private readonly NarratorService narratorService;
    private readonly PublisherService publisherService;
    private readonly SeriesService seriesService;
    private readonly StorageService storageService;
    private readonly TagService tagService;
    private readonly UserService userService;

    public AudibleDownloadManager(BookService bookService,
                                  AuthorService authorService,
                                  NarratorService narratorService,
                                  CategoryService categoryService,
                                  TagService tagService,
                                  SeriesService seriesService,
                                  UserService userService,
                                  PublisherService publisherService,
                                  StorageService storageService,
                                  DownloadService downloadService,
                                  DownloadQueue downloadQueue,
                                  AudibleDataGetter dataGetter) {
        this.bookService = bookService;
        this.authorService = authorService;
        this.narratorService = narratorService;
        this.categoryService = categoryService;
        this.tagService = tagService;
        this.seriesService = seriesService;
        this.userService = userService;
        this.publisherService = publisherService;
        this.storageService = storageService;
        this.downloadService = downloadService;
        this.downloadQueue = downloadQueue;
        this.dataGetter = dataGetter;
    }

    public async Task DownloadBook(string asin, int? userId = null, bool addToUser = false, bool force = false) {
        try {
            if (asin == null) {
                log.Error("Failed to parse book missing ASIN", asin);
                throw new FatalException("Failed to parse book missing ASIN");
            }

            log.Info("Request to download book from asin {0}", asin);

            int bookId = 0;
            AudibleBook? existingBook = await bookService.getBookASIN(asin);
            bool shouldDownload = await ShouldDownload(existingBook, force);
            if (shouldDownload || existingBook == null) {
                AudibleBook? newBookReponse = await DownloadAndCreateBook(asin, userId);
                if (newBookReponse == null) {
                    log.Warn("Can not continue because the book could not be downloaded");
                    return;
                }

                bookId = newBookReponse.Id;
            }
            else {
                bookId = existingBook.Id;
            }

            if (userId != null && addToUser)
                await userService.AddBookToUser((int)userId, bookId);
            else
                log.Debug("No user id provided, not adding book to user");

            log.Debug("Finished downloading book " + asin);
        }
        catch (FatalException e) {
            // If the book is a temp book we need to delete it
            AudibleBook? book = await bookService.getBookASIN(asin);
            if (book != null && book.IsTemp) {
                log.Warn("Failed to parse book with fatal error, deleting the temp book");
                await bookService.DeleteBookAsin(asin);
            }
        }
    }

    public async Task DownloadSeries(string asin, int? userId, bool force) {
        log.Debug("Downloading series: " + asin);

        if (asin == null) {
            log.Error("Failed to parse series ASIN from url {0}", asin);
            throw new FatalException("Failed to parse series ASIN from url");
        }

        AudibleSeries? storedSeries = await seriesService.GetSeriesAsin(asin);
        if (storedSeries != null) await seriesService.SetSeriesChecked(storedSeries.Id);

        bool shouldDownload = ShouldDownloadSeries(storedSeries, force);
        if (!shouldDownload) {
            log.Info("No need to downloaded series, skipping");
            return;
        }

        DateTime start = DateTime.Now;
        ParseSeries parsedSeries = await dataGetter.ParseSeries(asin);
        log.Debug("Parsing series took: " + (DateTime.Now - start).TotalMilliseconds + " ms");
        storedSeries = await seriesService.SaveOrGetSeries(parsedSeries.Asin, parsedSeries.Name, parsedSeries.Link);

        // If nothing was updated, the flag would not have been set back to false
        await seriesService.SetSeriesShouldDownload(storedSeries.Id, false);

        log.Debug($"Series {storedSeries.Name} has {parsedSeries.Books.Count} books");

        foreach (ParseSeriesBook book in parsedSeries.Books) {
            if (book.Asin == null) {
                log.Warn("Book link was null, skipping book", JsonSerializer.Serialize(book));
                continue;
            }

            AudibleBook? savedBook = await bookService.getBookASIN(book.Asin);
            if (savedBook == null) {
                log.Debug("Book " + book.Asin + " not found in database, creating temp book");
                AudibleBook tempBook = await bookService.CreateTempBook(book.Asin);
                await seriesService.AddBookToSeries(tempBook.Id, storedSeries.Id, book.BookNumber, book.Sort);
                int? jobId = null;
                if (userId != null)
                    jobId = await userService
                               .CreateJob((int)userId, "book",
                                          JsonSerializer.Serialize(new BookData { Asin = book.Asin }));

                log.Info("Adding book to download queue: " + book.Asin);
                await downloadQueue.SendDownloadBook(book.Asin, jobId, userId);
            }
            else {
                bool series = await seriesService.IsBookInSeriesByAsin(savedBook.Id, storedSeries.Asin);

                if (!series) {
                    log.Debug($"Adding book {savedBook.Title} to series {storedSeries.Name}");
                    await seriesService.AddBookToSeries(savedBook.Id, storedSeries.Id, book.BookNumber, book.Sort);
                }
                else {
                    log.Debug($"Book {savedBook.Title} already exists in series: {storedSeries.Name}");
                    if (!string.IsNullOrWhiteSpace(book.BookNumber) || book.Sort != null) {
                        log.Debug($"Updating book number for book: {savedBook.Title} with number {book.BookNumber}");
                        await seriesService.UpdateSeriesBookData(savedBook.Id, storedSeries.Id, book.BookNumber, book.Sort);
                    }
                }
            }
        }
    }

    private bool ShouldDownloadSeries(AudibleSeries? storedSeries, bool force) {
        if (storedSeries == null) {
            log.Debug("Series should be downloaded because it is not in the database");
            return true;
        }

        if (storedSeries.ShouldDownload) {
            log.Debug("Series should be downloaded because it is marked as should download");
            return true;
        }

        if (storedSeries.LastChecked < DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds()) {
            log.Debug("Series should be downloaded because it has not been checked for 7 days");
            return true;
        }

        if (force) {
            log.Debug("Series should be downloaded because force is true");
            return true;
        }

        log.Debug("Series should not be downloaded");
        return false;
    }

    private async Task<bool> ShouldDownload(AudibleBook? existingBook, bool force) {
        if (existingBook == null) {
            log.Debug("Book did not exist in the database, downloading");
            return true;
        }

        if (existingBook.ShouldDownload) {
            log.Debug("Book should be downloaded because it is marked as should download");
            return true;
        }

        // is the book last updated older than 1 month (2,628,288)?
        if (existingBook.LastUpdated <= DateTimeOffset.Now.ToUnixTimeSeconds() - 2628288) {
            log.Debug("Book was updated more than 1 month ago, downloading");
            return true;
        }

        if (force) {
            log.Debug("Book should be downloaded because force is true");
            return true;
        }

        // We should check if we already have the image, if not, download it
        bool imageCheck = await storageService.HasImage(existingBook.Asin);
        if (!imageCheck) {
            log.Debug("Book is missing the image, we need to re-download");
            return true;
        }

        log.Debug("No need to download the book");
        return false;
    }

    private async Task<AudibleBook?> DownloadAndCreateBook(string asin, int? userId) {
        log.Debug("Downloading and create book from asin: " + asin);

        DateTime start = DateTime.Now;
        ParseAudioBook? book = await dataGetter.ParseBook(asin);
        log.Debug("Getting book data took: " + (DateTime.Now - start).TotalMilliseconds + " ms");
        if (book == null) {
            log.Debug("Was unable to parse the book from HTML");
            throw new FatalException("Failed to parse book from HTML");
        }

        int? publisherId = null;
        if (!string.IsNullOrWhiteSpace(book.Publisher)) {
            AudiblePublisher publisher = await publisherService.SaveOrGetPublisher(book.Publisher);
            publisherId = publisher.Id;
        }

        start = DateTime.Now;
        AudibleBook? newBook =
            await bookService.SaveBook(book.Asin, book.Isbn, book.Link, book.Title, book.RuntimeSeconds, book.Released, book.Summary,
                                       publisherId);
        log.Debug("Created or updated book with id: " + newBook.Id + " in " + (DateTime.Now - start).TotalMilliseconds +
                  " ms");

        if (newBook != null && book.Image != null) {
            start = DateTime.Now;
            await DownloadImageWithRetry(newBook, book.Image);
            log.Debug("Downloaded the image for book id: " + newBook.Id + " in " +
                      (DateTime.Now - start).TotalMilliseconds + " ms");
        }

        int bookId = newBook.Id;
        log.Debug(
                  $"Book had {book.Series.Count} series, {book.Authors.Count} authors, {book.Narrators.Count} narrators, {book.Categories.Count} categories, {book.Tags.Count} tags");

        foreach (ParseAudioBookSeries series in book.Series)
            try {
                bool newSeries = await seriesService.GetSeriesAsin(series.Asin) == null;
                AudibleSeries savedSeries =
                    await seriesService.SaveOrGetSeries(series.Asin, series.Name, series.Link);
                await seriesService.AddBookToSeries(bookId, savedSeries.Id, series.BookNumber, series.Sort);

                if (newSeries || !savedSeries.ShouldDownload) {
                    await seriesService.SetSeriesShouldDownload(savedSeries.Id, true);
                    int? jobId = null;
                    if (userId != null)
                        jobId = await userService.CreateJob((int)userId, "series",
                                                            JsonSerializer.Serialize(new SeriesData
                                                                                     { Asin = savedSeries.Asin }));
                    log.Info("Adding series to download queue: " + savedSeries.Asin);
                    await downloadQueue.SendDownloadSeries(savedSeries.Asin, jobId, userId);
                }
            }
            catch (Exception e) {
                log.Error("Failed to process series {0} for book {1}", series.Name, book.Title);
                throw e;
            }

        foreach (ParseAudioBookPerson author in book.Authors) {
            AudibleAuthor savedAuthor = await authorService.SaveOrGetAuthor(author.Asin, author.Name, author.Link);
            await authorService.AddBookToAuthor(bookId, savedAuthor);
        }

        foreach (string tag in book.Tags) {
            AudibleTag savedTag = await tagService.SaveOrGetTag(tag);
            await tagService.AddTagToBook(bookId, savedTag);
        }

        foreach (string narrator in book.Narrators) {
            AudibleNarrator savedNarrator = await narratorService.SaveOrGetNarrator(narrator);
            await narratorService.AddNarratorToBook(bookId, savedNarrator);
        }

        foreach (ParseAudioBookCategory category in book.Categories) {
            AudibleCategory savedCategory = await categoryService.SaveOrGetCategory(category.Name, category.Link);
            await categoryService.AddCategoryToBook(bookId, savedCategory);
        }

        return newBook;
    }

    private async Task DownloadImageWithRetry(AudibleBook newBook, string imageUrl) {
        try {
            await DownloadImage(newBook, imageUrl);
        }
        catch (RetryableException e) {
            log.Warn("Failed to download image, retrying after 1 sec", e);
            await Task.Delay(1000);
            await DownloadImage(newBook, imageUrl);
        }
    }

    private async Task DownloadImage(AudibleBook newBook, string imageUrl) {
        log.Debug("Downloading book image");
        if (!await storageService.HasImage(newBook.Asin)) {
            log.Debug("Book image does not exist, downloading");
            await downloadService.DownloadImage(imageUrl, newBook.Asin);
        }
        else {
            log.Debug("Book image already exists, skipping download");
        }
    }
}