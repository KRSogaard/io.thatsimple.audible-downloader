namespace AudibleDownloader.DAL.Models;

public partial class Series
{
    public int Id { get; set; }

    public string? Asin { get; set; }

    public string? Link { get; set; }

    public string? Name { get; set; }

    public int? LastUpdated { get; set; }

    public int? Created { get; set; }

    public string? Summary { get; set; }

    public bool? ShouldDownload { get; set; }

    public int? LastChecked { get; set; }

    public virtual ICollection<SeriesBook> SeriesBooks { get; } = new List<SeriesBook>();

    public virtual ICollection<UsersArchivedSeries> UsersArchivedSeries { get; } = new List<UsersArchivedSeries>();
}
