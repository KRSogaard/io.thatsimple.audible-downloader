namespace AudibleDownloader.DAL.Models;

public class Narrator {
    public int Id { get; set; }
    public string Name { get; set; }
    public long Created { get; set; }

    public virtual ICollection<NarratorsBook> NarratorsBooks { get; } = new List<NarratorsBook>();
}