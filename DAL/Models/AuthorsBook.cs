namespace AudibleDownloader.DAL.Models;

public partial class AuthorsBook
{
    public int Id { get; set; }

    public int? BookId { get; set; }

    public int? AuthorId { get; set; }

    public int? Created { get; set; }

    public virtual Author? Author { get; set; }

    public virtual Book? Book { get; set; }
}
