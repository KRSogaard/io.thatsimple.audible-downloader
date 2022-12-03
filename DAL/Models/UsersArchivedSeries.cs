namespace AudibleDownloader.DAL.Models;

public partial class UsersArchivedSeries
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SeriesId { get; set; }

    public long Created { get; set; }

    public virtual Series Series { get; set; }

    public virtual User User { get; set; }
}
