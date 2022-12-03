namespace AudibleDownloader.DAL.Models;

public partial class TagsBook
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int TagId { get; set; }

    public long Created { get; set; }

    public virtual Book Book { get; set; }

    public virtual Tag Tag { get; set; }
}
