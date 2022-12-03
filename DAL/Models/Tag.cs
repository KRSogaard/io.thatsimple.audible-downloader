namespace AudibleDownloader.DAL.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; }

    public long Created { get; set; }

    public virtual ICollection<TagsBook> TagsBooks { get; } = new List<TagsBook>();
}
