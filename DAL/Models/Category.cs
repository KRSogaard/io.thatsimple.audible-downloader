namespace AudibleDownloader.DAL.Models;

public class Category {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public long Created { get; set; }

    public virtual ICollection<CategoriesBook> CategoriesBooks { get; } = new List<CategoriesBook>();
}