namespace AudibleDownloader.DAL.Models;

public partial class UsersJob
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public long Created { get; set; }

    public string Type { get; set; }

    public string? Payload { get; set; }

    public virtual User User { get; set; }
}
