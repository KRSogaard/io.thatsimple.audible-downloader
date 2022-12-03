using System.Net;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Services;
using NLog;

namespace AudibleDownloader.Parser;

public class AudibleScraperDataGetter : AudibleDataGetter {
    private readonly DownloadService downloadService;
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public AudibleScraperDataGetter(DownloadService downloadService) {
        this.downloadService = downloadService;
    }

    public async Task<ParseAudioBook> ParseBook(string asin) {
        string url = $"https://www.audible.com/pd/{asin}";
        DownloadResponse? downloadResponse = await downloadService.DownloadHtml(url, message => {
            if (message.Headers.Location != null) {
                url = "https://www.audible.com" + message.Headers.Location;
                if (url.StartsWith("https://www.audible.com/pd/") && url.EndsWith(asin)) return true;
                log.Debug("Redirection to unexpected url " + url + " from " + url);
            }
            else {
                log.Debug("Redirect without location header, not allowing redirect");
            }

            return false;
        });
        if (downloadResponse == null || downloadResponse.StatusCode != HttpStatusCode.OK) {
            log.Warn("Failed to download book retrying after 1 sec: " + url);
            await Task.Delay(1000);
            downloadResponse = await downloadService.DownloadHtml(url);
        }

        if (downloadResponse != null && downloadResponse.StatusCode == HttpStatusCode.NotFound) {
            log.Error("Book no longer exists, skipping download: " + url);
            throw new FatalException("Book no longer exists");
        }

        if (downloadResponse != null && downloadResponse.StatusCode == HttpStatusCode.InternalServerError) {
            log.Error("Download returned 500 error: " + url);
            throw new RetryableException("Failed to download");
        }

        if (downloadResponse == null || downloadResponse.StatusCode != HttpStatusCode.OK) {
            log.Error("Failed to download book with unknown status code " + downloadResponse?.StatusCode + ": " + url);
            throw new RetryableException("Failed to download");
        }

        string? html = downloadResponse.Data;
        if (html == null || html.Length < 100) {
            log.Error("Failed to download book, HTML was empty: " + url);
            throw new RetryableException();
        }

        return await AudibleParser.ParseBook(html);
    }

    public async Task<ParseSeries> ParseSeries(string asin) {
        string url = "https://www.audible.com/series/" + asin;
        log.Info("Downloading series");
        DownloadResponse? downloadResponse = await downloadService.DownloadHtml(url, message => {
            if (message.Headers.Location != null) {
                url = "https://www.audible.com" + message.Headers.Location;
                if (url.StartsWith("https://www.audible.com/series/") && url.EndsWith(asin)) return true;
                log.Debug("Redirection to unexpected url " + url + " from " + url);
            }
            else {
                log.Debug("Redirect without location header, not allowing redirect");
            }

            return false;
        });
        if (downloadResponse == null || downloadResponse.StatusCode != HttpStatusCode.OK) {
            log.Warn("Failed to download series retrying after 1 sec: " + url);
            await Task.Delay(1000);
            downloadResponse = await downloadService.DownloadHtml(url);
        }

        if (downloadResponse != null && downloadResponse.StatusCode == HttpStatusCode.NotFound) {
            log.Error("Series no longer exists, skipping download: " + url);
            throw new FatalException("Series no longer exists");
        }

        if (downloadResponse != null && downloadResponse.StatusCode == HttpStatusCode.InternalServerError) {
            log.Error("Download returned 500 error: " + url);
            throw new RetryableException();
        }

        if (downloadResponse == null || downloadResponse.StatusCode != HttpStatusCode.OK) {
            log.Error("Failed to download series with unknown status code " + downloadResponse?.StatusCode + ": " +
                      url);
            throw new RetryableException("Failed to download");
        }

        string? html = downloadResponse.Data;
        if (html == null || html.Length < 100) {
            log.Error("Failed to download book, HTML was empty: " + url);
            throw new RetryableException();
        }

        return await AudibleParser.ParseSeries(html);
    }
}