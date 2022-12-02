namespace AudibleDownloader.DAL.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string? Tag1 { get; set; }

    public int? Created { get; set; }

    public virtual ICollection<TagsBook> TagsBooks { get; } = new List<TagsBook>();
}
