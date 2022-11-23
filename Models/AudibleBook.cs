namespace AudibleDownloader.Models {
  public class AudibleBook {
      public int? id {get; set; }
      public string asin {get; set; }
      public string link {get; set; }
      public string title {get; set; }
      public int length {get; set; }
      public DateTime released {get; set; }
      public string summary {get; set; }
      public DateTime? lastUpdated {get; set; }
      public AudibleSeriesBook[]? series {get; set; }
      public AudibleAuthor[]? authors {get; set; }
      public string[]? tags {get; set; }
      public AudibleNarrator[]? narrators {get; set; }
      public AudibleCategory[]? categories {get; set; }
  }

  public class AudibleSeries {
    public int? id {get; set; }
    public string asin {get; set; }
    public string link {get; set; }
    public string summary {get; set; }
    public string name {get; set; }
    public DateTime? lastUpdated {get; set; }
    public bool? shouldDownload {get; set; }
  }

  public class AudibleSeriesBook : AudibleSeries {
    public string bookNumber {get; set; }
  }

  public class AudibleSeriesWithBooks : AudibleSeries {
    public int[]? books {get; set; }
  }

  public class AudibleAuthor {
    public int? id {get; set; }
    public string asin {get; set; }
    public string link {get; set; }
    public string name {get; set; }
  }

  public class AudibleNarrator {
    public int? id {get; set; }
    public string name {get; set; }
  }

  public class AudibleCategory {
    public int? id {get; set; }
    public string name {get; set; }
    public string link {get; set; }
  }
}
