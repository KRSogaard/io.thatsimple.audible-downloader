namespace AudibleDownloader.DAL.Models;

public class NarratorsBook {
    public int Id { get; set; }
    public int NarratorId { get; set; }
    public int BookId { get; set; }
    public long Created { get; set; }

    public virtual Book Book { get; set; }
    public virtual Narrator Narrator { get; set; }
}