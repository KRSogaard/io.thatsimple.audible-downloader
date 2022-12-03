namespace AudibleDownloader.DAL.Models;

public class UsersBook {
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public long Created { get; set; }

    public virtual Book Book { get; set; }

    public virtual User User { get; set; }
}