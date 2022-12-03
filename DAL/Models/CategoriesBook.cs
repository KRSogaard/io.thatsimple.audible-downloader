namespace AudibleDownloader.DAL.Models;

public class CategoriesBook {
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int BookId { get; set; }
    public long Created { get; set; }

    public virtual Book Book { get; set; }
    public virtual Category Category { get; set; }
}