using System;
using System.Collections.Generic;
using AudibleDownloader.DAL.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using AudibleDownloader.DAL.Models;

namespace AudibleDownloader.DAL;

public partial class AudibleContext : DbContext
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();
    private static string connectionString;
    
    public AudibleContext()
    {
    }

    public AudibleContext(DbContextOptions<AudibleContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<AuthorsBook> AuthorsBooks { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<CategoriesBook> CategoriesBooks { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Narrator> Narrators { get; set; }

    public virtual DbSet<NarratorsBook> NarratorsBooks { get; set; }

    public virtual DbSet<Series> Series { get; set; }

    public virtual DbSet<SeriesBook> SeriesBooks { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagsBook> TagsBooks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersArchivedSeries> UsersArchivedSeries { get; set; }

    public virtual DbSet<UsersBook> UsersBooks { get; set; }

    public virtual DbSet<UsersJob> UsersJobs { get; set; }

    public virtual DbSet<UsersToken> UsersTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(message => log.Debug(message));
        
        if (String.IsNullOrWhiteSpace(connectionString))
        {
            var host = Config.Get("DB_HOST");
            var port = Config.Get("DB_PORT");
            var user = Config.Get("DB_USER");
            var password = Config.Get("DB_PASSWORD");
            var database = Config.Get("DB_NAME");
            connectionString =
                $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};Pooling=True;";
            log.Info("Creating new MySQL connection to \"{0}:{1}\" user: \"{2}\", database: \"{3}\"", host, port, user,
                database);
        }
        
        optionsBuilder.UseMySql(
            connectionString,
            MySqlServerVersion.LatestSupportedServerVersion);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("authors");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Asin)
                .HasMaxLength(128)
                .HasColumnName("asin")
                .IsRequired();
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Link)
                .HasMaxLength(512)
                .HasColumnName("link");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<AuthorsBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("authors_books");

            entity.HasIndex(e => e.AuthorId, "author_id");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.AuthorId)
                .HasColumnType("int(11)")
                .HasColumnName("author_id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");

            entity.HasOne(d => d.Author).WithMany(p => p.AuthorsBooks)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("authors_books_ibfk_2");

            entity.HasOne(d => d.Book).WithMany(p => p.AuthorsBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("authors_books_ibfk_1");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("books");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Asin)
                .HasMaxLength(128)
                .HasColumnName("asin");
            entity.Property(e => e.AuthorsCache)
                .HasColumnType("text")
                .HasColumnName("authors_cache");
            entity.Property(e => e.CategoriesCache)
                .HasColumnType("text")
                .HasColumnName("categories_cache");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.LastUpdated)
                .HasColumnType("int(11)")
                .HasColumnName("last_updated");
            entity.Property(e => e.Length)
                .HasColumnType("int(11)")
                .HasColumnName("length");
            entity.Property(e => e.Link)
                .HasColumnType("text")
                .HasColumnName("link");
            entity.Property(e => e.NarratorsCache)
                .HasColumnType("text")
                .HasColumnName("narrators_cache");
            entity.Property(e => e.Released)
                .HasColumnType("int(11)")
                .HasColumnName("released");
            entity.Property(e => e.ShouldDownload).HasColumnName("should_download");
            entity.Property(e => e.Summary)
                .HasColumnType("text")
                .HasColumnName("summary");
            entity.Property(e => e.TagsCache)
                .HasColumnType("text")
                .HasColumnName("tags_cache");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<CategoriesBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categories_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.CategoryId)
                .HasColumnType("int(11)")
                .HasColumnName("category_id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");

            entity.HasOne(d => d.Book).WithMany(p => p.CategoriesBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("categories_books_ibfk_1");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoriesBooks)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("categories_books_ibfk_2");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Link)
                .HasMaxLength(512)
                .HasColumnName("link");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Narrator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("narrators");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<NarratorsBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("narrators_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.NarratorId, "narrator_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.NarratorId)
                .HasColumnType("int(11)")
                .HasColumnName("narrator_id");

            entity.HasOne(d => d.Book).WithMany(p => p.NarratorsBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("narrators_books_ibfk_1");

            entity.HasOne(d => d.Narrator).WithMany(p => p.NarratorsBooks)
                .HasForeignKey(d => d.NarratorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("narrators_books_ibfk_2");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("series");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Asin)
                .HasMaxLength(128)
                .HasColumnName("asin");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.LastChecked)
                .HasColumnType("int(11)")
                .HasColumnName("last_checked");
            entity.Property(e => e.LastUpdated)
                .HasColumnType("int(11)")
                .HasColumnName("last_updated");
            entity.Property(e => e.Link)
                .HasMaxLength(512)
                .HasColumnName("link");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.ShouldDownload).HasColumnName("should_download");
            entity.Property(e => e.Summary)
                .HasColumnType("text")
                .HasColumnName("summary");
        });

        modelBuilder.Entity<SeriesBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("series_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.SeriesId, "series_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.BookNumber)
                .HasMaxLength(64)
                .HasColumnName("book_number");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.SeriesId)
                .HasColumnType("int(11)")
                .HasColumnName("series_id");

            entity.HasOne(d => d.Book).WithMany(p => p.SeriesBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("series_books_ibfk_1");

            entity.HasOne(d => d.Series).WithMany(p => p.SeriesBooks)
                .HasForeignKey(d => d.SeriesId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("series_books_ibfk_2");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tags");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Tag1)
                .HasMaxLength(128)
                .HasColumnName("tag");
        });

        modelBuilder.Entity<TagsBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tags_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.TagId, "tag_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.TagId)
                .HasColumnType("int(11)")
                .HasColumnName("tag_id");

            entity.HasOne(d => d.Book).WithMany(p => p.TagsBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tags_books_ibfk_1");

            entity.HasOne(d => d.Tag).WithMany(p => p.TagsBooks)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tags_books_ibfk_2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(20)
                .HasColumnName("password_salt");
            entity.Property(e => e.Username)
                .HasMaxLength(128)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UsersArchivedSeries>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_archived_series");

            entity.HasIndex(e => e.SeriesId, "series_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.SeriesId)
                .HasColumnType("int(11)")
                .HasColumnName("series_id");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Series).WithMany(p => p.UsersArchivedSeries)
                .HasForeignKey(d => d.SeriesId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("users_archived_series_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UsersArchivedSeries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("users_archived_series_ibfk_1");
        });

        modelBuilder.Entity<UsersBook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.BookId)
                .HasColumnType("int(11)")
                .HasColumnName("book_id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany(p => p.UsersBooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("book_id");

            entity.HasOne(d => d.User).WithMany(p => p.UsersBooks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_id");
        });

        modelBuilder.Entity<UsersJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_jobs");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Payload)
                .HasColumnType("text")
                .HasColumnName("payload");
            entity.Property(e => e.Type)
                .HasMaxLength(128)
                .HasColumnName("type");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UsersJobs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("users_jobs_ibfk_1");
        });

        modelBuilder.Entity<UsersToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_tokens");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("int(11)")
                .HasColumnName("created");
            entity.Property(e => e.Expires)
                .HasColumnType("int(11)")
                .HasColumnName("expires");
            entity.Property(e => e.Token)
                .HasMaxLength(128)
                .HasColumnName("token");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UsersTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("users_tokens_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
