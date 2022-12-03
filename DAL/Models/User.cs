namespace AudibleDownloader.DAL.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string PasswordSalt { get; set; }

    public long Created { get; set; }

    public string Email { get; set; }

    public virtual ICollection<UsersArchivedSeries> UsersArchivedSeries { get; } = new List<UsersArchivedSeries>();

    public virtual ICollection<UsersBook> UsersBooks { get; } = new List<UsersBook>();

    public virtual ICollection<UsersJob> UsersJobs { get; } = new List<UsersJob>();

    public virtual ICollection<UsersToken> UsersTokens { get; } = new List<UsersToken>();
}
