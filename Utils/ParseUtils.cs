namespace AudibleDownloader {
  public static class ParseUtils {
    public static string? GetASINFromUrl(string url) {
      return url.Split('?')[0]?.Split('/')?.Last();
    }
  }
}
