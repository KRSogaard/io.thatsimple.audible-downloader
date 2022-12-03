namespace AudibleDownloader.DAL.Models;

public class UsersToken {
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; }

    public long Created { get; set; }

    public long Expires { get; set; }

    public virtual User User { get; set; }
}