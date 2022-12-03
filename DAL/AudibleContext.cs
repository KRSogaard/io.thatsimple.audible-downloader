using AudibleDownloader.DAL.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace AudibleDownloader.DAL;

public partial class AudibleContext : DbContext {
    private static string connectionString;
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public AudibleContext() { }

    public AudibleContext(DbContextOptions<AudibleContext> options)
        : base(options) { }

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

    public virtual DbSet<Publisher> Publishers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.LogTo(message => log.Trace(message));

        if (string.IsNullOrWhiteSpace(connectionString)) {
            string? host = Config.Get("DB_HOST");
            string? port = Config.Get("DB_PORT");
            string? user = Config.Get("DB_USER");
            string? password = Config.Get("DB_PASSWORD");
            string? database = Config.Get("DB_NAME");
            connectionString =
                $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};Pooling=True;";
            log.Info("Creating new MySQL connection to \"{0}:{1}\" user: \"{2}\", database: \"{3}\"", host, port, user,
                     database);
        }

        optionsBuilder.UseMySql(
                                connectionString,
                                MySqlServerVersion.LatestSupportedServerVersion);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder
           .UseCollation("utf8mb4_unicode_ci")
           .HasCharSet("utf8mb4");

        modelBuilder.Entity<Author>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("authors");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Asin)
                  .HasMaxLength(128)
                  .HasColumnName("asin")
                  .HasColumnOrder(1);
            entity.Property(e => e.Name)
                  .HasMaxLength(128)
                  .HasColumnName("name")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Link)
                  .HasMaxLength(512)
                  .HasColumnName("link")
                  .HasColumnOrder(3);
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(4)
                  .IsRequired();
        });

        modelBuilder.Entity<AuthorsBook>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("authors_books");
            entity.HasIndex(e => e.AuthorId, "author_id");
            entity.HasIndex(e => e.BookId, "book_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.AuthorId)
                  .HasColumnType("int(11)")
                  .HasColumnName("author_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.BookId)
                  .HasColumnType("int(11)")
                  .HasColumnName("book_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();

            entity.HasOne(d => d.Author).WithMany(p => p.AuthorsBooks)
                  .HasForeignKey(d => d.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("authors_books_ibfk_2");

            entity.HasOne(d => d.Book).WithMany(p => p.AuthorsBooks)
                  .HasForeignKey(d => d.BookId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("authors_books_ibfk_1");
        });

        modelBuilder.Entity<Book>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("books");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Asin)
                  .HasMaxLength(128)
                  .HasColumnName("asin")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Isbn)
                  .HasColumnType("bigint")
                  .HasColumnName("isbn")
                  .HasColumnOrder(2);
            entity.Property(e => e.Title)
                  .HasMaxLength(255)
                  .HasColumnName("title")
                  .HasColumnOrder(3);
            entity.Property(e => e.Length)
                  .HasColumnType("int(11)")
                  .HasColumnName("length")
                  .HasColumnOrder(4);
            entity.Property(e => e.Link)
                  .HasColumnType("text")
                  .HasColumnName("link")
                  .HasColumnOrder(5);
            entity.Property(e => e.Released)
                  .HasColumnType("int(11)")
                  .HasColumnName("released")
                  .HasColumnOrder(6);
            entity.Property(e => e.Summary)
                  .HasColumnType("text")
                  .HasColumnName("summary")
                  .HasColumnOrder(7);
            entity.Property(e => e.PublisherId)
                  .HasColumnType("int(11)")
                  .HasColumnName("publisher_id")
                  .HasColumnOrder(8);
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(9)
                  .IsRequired();
            entity.Property(e => e.LastUpdated)
                  .HasColumnType("int(11)")
                  .HasColumnName("last_updated")
                  .HasColumnOrder(10)
                  .IsRequired();
            entity.Property(e => e.NarratorsCache)
                  .HasColumnType("text")
                  .HasColumnName("narrators_cache")
                  .HasColumnOrder(11);
            entity.Property(e => e.TagsCache)
                  .HasColumnType("text")
                  .HasColumnName("tags_cache")
                  .HasColumnOrder(12);
            entity.Property(e => e.AuthorsCache)
                  .HasColumnType("text")
                  .HasColumnName("authors_cache")
                  .HasColumnOrder(13);
            entity.Property(e => e.CategoriesCache)
                  .HasColumnType("text")
                  .HasColumnName("categories_cache")
                  .HasColumnOrder(14);
            entity.Property(e => e.ShouldDownload)
                  .HasColumnName("should_download")
                  .HasColumnOrder(15);
            entity.Property(e => e.IsTemp)
                  .HasColumnName("is_temp")
                  .HasColumnOrder(16);

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                  .HasForeignKey(d => d.PublisherId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("books_ibfk_1");
        });

        modelBuilder.Entity<CategoriesBook>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categories_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.CategoryId)
                  .HasColumnType("int(11)")
                  .HasColumnName("category_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.BookId)
                  .HasColumnType("int(11)")
                  .HasColumnName("book_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.CategoriesBooks)
                  .HasForeignKey(d => d.BookId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("categories_books_ibfk_1");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoriesBooks)
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("categories_books_ibfk_2");
        });

        modelBuilder.Entity<Category>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                  .HasMaxLength(128)
                  .HasColumnName("name")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Link)
                  .HasMaxLength(512)
                  .HasColumnName("link")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();
        });

        modelBuilder.Entity<Narrator>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("narrators");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                  .HasMaxLength(128)
                  .HasColumnName("name")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(2)
                  .IsRequired();
        });

        modelBuilder.Entity<NarratorsBook>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("narrators_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.NarratorId, "narrator_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.NarratorId)
                  .HasColumnType("int(11)")
                  .HasColumnName("narrator_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.BookId)
                  .HasColumnType("int(11)")
                  .HasColumnName("book_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.NarratorsBooks)
                  .HasForeignKey(d => d.BookId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("narrators_books_ibfk_1");

            entity.HasOne(d => d.Narrator).WithMany(p => p.NarratorsBooks)
                  .HasForeignKey(d => d.NarratorId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("narrators_books_ibfk_2");
        });

        modelBuilder.Entity<Series>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("series");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Asin)
                  .HasMaxLength(128)
                  .HasColumnName("asin")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Name)
                  .HasMaxLength(128)
                  .HasColumnName("name")
                  .HasColumnOrder(2);
            entity.Property(e => e.Link)
                  .HasMaxLength(512)
                  .HasColumnName("link")
                  .HasColumnOrder(3);
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(4)
                  .IsRequired();
            entity.Property(e => e.LastUpdated)
                  .HasColumnType("int(11)")
                  .HasColumnName("last_updated")
                  .HasColumnOrder(5)
                  .IsRequired();
            entity.Property(e => e.LastChecked)
                  .HasColumnType("int(11)")
                  .HasColumnName("last_checked")
                  .HasColumnOrder(6)
                  .IsRequired();
            entity.Property(e => e.ShouldDownload)
                  .HasColumnName("should_download")
                  .HasColumnOrder(7)
                  .IsRequired();
        });

        modelBuilder.Entity<SeriesBook>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("series_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.SeriesId, "series_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.SeriesId)
                  .HasColumnType("int(11)")
                  .HasColumnName("series_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.BookId)
                  .HasColumnType("int(11)")
                  .HasColumnName("book_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.BookNumber)
                  .HasMaxLength(64)
                  .HasColumnName("book_number")
                  .HasColumnOrder(3);
            entity.Property(e => e.Sort)
                  .HasMaxLength(64)
                  .HasColumnName("sort")
                  .HasColumnOrder(4);
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(5)
                  .IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.SeriesBooks)
                  .HasForeignKey(d => d.BookId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("series_books_ibfk_1");

            entity.HasOne(d => d.Series).WithMany(p => p.SeriesBooks)
                  .HasForeignKey(d => d.SeriesId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("series_books_ibfk_2");
        });

        modelBuilder.Entity<Tag>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tags");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                  .HasMaxLength(128)
                  .HasColumnName("tag")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(2)
                  .IsRequired();
        });

        modelBuilder.Entity<TagsBook>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tags_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.TagId, "tag_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.TagId)
                  .HasColumnType("int(11)")
                  .HasColumnName("tag_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.BookId)
                  .HasColumnType("int(11)")
                  .HasColumnName("book_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.TagsBooks)
                  .HasForeignKey(d => d.BookId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("tags_books_ibfk_1");

            entity.HasOne(d => d.Tag).WithMany(p => p.TagsBooks)
                  .HasForeignKey(d => d.TagId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("tags_books_ibfk_2");
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Email)
                  .HasMaxLength(256)
                  .HasColumnName("email")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Username)
                  .HasMaxLength(128)
                  .HasColumnName("username")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Password)
                  .HasMaxLength(128)
                  .HasColumnName("password")
                  .HasColumnOrder(3)
                  .IsRequired();
            entity.Property(e => e.PasswordSalt)
                  .HasMaxLength(20)
                  .HasColumnName("password_salt")
                  .HasColumnOrder(4)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(5)
                  .IsRequired();
        });

        modelBuilder.Entity<UsersArchivedSeries>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_archived_series");

            entity.HasIndex(e => e.SeriesId, "series_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.UserId)
                  .HasColumnType("int(11)")
                  .HasColumnName("user_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.SeriesId)
                  .HasColumnType("int(11)")
                  .HasColumnName("series_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();

            entity.HasOne(d => d.Series).WithMany(p => p.UsersArchivedSeries)
                  .HasForeignKey(d => d.SeriesId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("users_archived_series_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UsersArchivedSeries)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("users_archived_series_ibfk_1");
        });

        modelBuilder.Entity<UsersBook>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_books");

            entity.HasIndex(e => e.BookId, "book_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.UserId)
                  .HasColumnType("int(11)")
                  .HasColumnName("user_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.BookId)
                  .HasColumnType("int(11)")
                  .HasColumnName("book_id")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.UsersBooks)
                  .HasForeignKey(d => d.BookId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("book_id");

            entity.HasOne(d => d.User).WithMany(p => p.UsersBooks)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("user_id");
        });

        modelBuilder.Entity<UsersJob>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_jobs");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.UserId)
                  .HasColumnType("int(11)")
                  .HasColumnName("user_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Type)
                  .HasMaxLength(128)
                  .HasColumnName("type")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Payload)
                  .HasColumnType("text")
                  .HasColumnName("payload")
                  .HasColumnOrder(3);
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(4)
                  .IsRequired();

            entity.HasOne(d => d.User).WithMany(p => p.UsersJobs)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("users_jobs_ibfk_1");
        });

        modelBuilder.Entity<UsersToken>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_tokens");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.UserId)
                  .HasColumnType("int(11)")
                  .HasColumnName("user_id")
                  .HasColumnOrder(1)
                  .IsRequired();
            entity.Property(e => e.Token)
                  .HasMaxLength(128)
                  .HasColumnName("token")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(3)
                  .IsRequired();
            entity.Property(e => e.Expires)
                  .HasColumnType("int(11)")
                  .HasColumnName("expires")
                  .HasColumnOrder(4)
                  .IsRequired();

            entity.HasOne(d => d.User).WithMany(p => p.UsersTokens)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("users_tokens_ibfk_1");
        });

        modelBuilder.Entity<Publisher>(entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("publishers");

            entity.Property(e => e.Id)
                  .HasColumnType("int(11)")
                  .HasColumnName("id")
                  .HasColumnOrder(0)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                  .HasMaxLength(128)
                  .HasColumnName("name")
                  .HasColumnOrder(2)
                  .IsRequired();
            entity.Property(e => e.Created)
                  .HasColumnType("int(11)")
                  .HasColumnName("created")
                  .HasColumnOrder(4)
                  .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}