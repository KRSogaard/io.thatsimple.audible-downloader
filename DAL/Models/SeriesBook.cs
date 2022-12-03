namespace AudibleDownloader.DAL.Models;

public partial class SeriesBook
{
    public int Id { get; set; }

    public int SeriesId { get; set; }

    public int BookId { get; set; }

    public string? BookNumber { get; set; }

    public int? Sort { get; set; }

    public long Created { get; set; }

    public virtual Book Book { get; set; }

    public virtual Series Series { get; set; }
}
