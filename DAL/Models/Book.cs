namespace AudibleDownloader.DAL.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Asin { get; set; }
    public long? Isbn { get; set; }
    public string? Title { get; set; }
    public int? Length { get; set; }
    public string? Link { get; set; }
    public long? Released { get; set; }
    public string? Summary { get; set; }
    public long LastUpdated { get; set; }
    public long Created { get; set; }

    public int? PublisherId { get; set; }
    public string? CategoriesCache { get; set; }
    public string? TagsCache { get; set; }
    public string? NarratorsCache { get; set; }
    public string? AuthorsCache { get; set; }
    public bool ShouldDownload { get; set; }
    public bool IsTemp { get; set; }

    public virtual ICollection<AuthorsBook> AuthorsBooks { get; } = new List<AuthorsBook>();

    public virtual ICollection<CategoriesBook> CategoriesBooks { get; } = new List<CategoriesBook>();

    public virtual ICollection<NarratorsBook> NarratorsBooks { get; } = new List<NarratorsBook>();

    public virtual ICollection<SeriesBook> SeriesBooks { get; } = new List<SeriesBook>();

    public virtual ICollection<TagsBook> TagsBooks { get; } = new List<TagsBook>();

    public virtual ICollection<UsersBook> UsersBooks { get; } = new List<UsersBook>();

    public virtual Publisher? Publisher { get; }
}
