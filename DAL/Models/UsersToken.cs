namespace AudibleDownloader.DAL.Models;

public partial class UsersToken
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Token { get; set; }

    public int? Created { get; set; }

    public int? Expires { get; set; }

    public virtual User? User { get; set; }
}
