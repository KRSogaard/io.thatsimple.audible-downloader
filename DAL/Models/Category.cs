namespace AudibleDownloader.DAL.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Link { get; set; }

    public int? Created { get; set; }

    public virtual ICollection<CategoriesBook> CategoriesBooks { get; } = new List<CategoriesBook>();
}
