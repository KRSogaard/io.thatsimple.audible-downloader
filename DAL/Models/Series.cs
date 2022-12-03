namespace AudibleDownloader.DAL.Models;

public partial class Series
{
    public int Id { get; set; }

    public string Asin { get; set; }

    public string? Link { get; set; }

    public string? Name { get; set; }

    public long LastUpdated { get; set; }

    public long Created { get; set; }

    public bool ShouldDownload { get; set; }

    public long LastChecked { get; set; }

    public virtual ICollection<SeriesBook> SeriesBooks { get; } = new List<SeriesBook>();

    public virtual ICollection<UsersArchivedSeries> UsersArchivedSeries { get; } = new List<UsersArchivedSeries>();
}
