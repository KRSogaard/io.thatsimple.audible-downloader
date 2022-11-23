using AudibleDownloader.Exceptions;
using NLog;

namespace AudibleDownloader {
  public class AudibleDownloadManager {
    Logger log = LogManager.GetCurrentClassLogger();
    public async Task DownloadBook(string url, string? userId, bool addToUser, bool force) {
        log.Info("Request to download book from url {0}", url);
        string bookASIN = ParseUtils.GetASINFromUrl(url);
        if (bookASIN == null) {
            log.Error("Failed to parse book ASIN from url {0}", url);
            throw new FatalException("Failed to parse book ASIN from url");
        }

        string bookId = null;


    }

    public async Task DownloadSeries(string url, string? userId, bool force) {
        await Task.Delay(100);
    }
  }
}
