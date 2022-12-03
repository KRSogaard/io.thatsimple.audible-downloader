namespace AudibleDownloader.DAL.Models;

public class Publisher {
    public int Id { get; set; }

    public string Name { get; set; }

    public long Created { get; set; }

    public virtual ICollection<Book> Books { get; } = new List<Book>();
}