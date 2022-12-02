namespace AudibleDownloader.DAL.Models;

public partial class Narrator
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Created { get; set; }

    public virtual ICollection<NarratorsBook> NarratorsBooks { get; } = new List<NarratorsBook>();
}
