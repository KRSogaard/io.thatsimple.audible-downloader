namespace AudibleDownloader.DAL.Models;

public partial class Author
{
    public int Id { get; set; }

    public string? Asin { get; set; }

    public string? Link { get; set; }

    public string Name { get; set; }

    public long Created { get; set; }

    public virtual ICollection<AuthorsBook> AuthorsBooks { get; } = new List<AuthorsBook>();
}
