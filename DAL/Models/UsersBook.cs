﻿namespace AudibleDownloader.DAL.Models;

public partial class UsersBook
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? BookId { get; set; }

    public int? Created { get; set; }

    public virtual Book? Book { get; set; }

    public virtual User? User { get; set; }
}