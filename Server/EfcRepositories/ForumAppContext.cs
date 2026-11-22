namespace EfcRepositories;

using Entities;
using Microsoft.EntityFrameworkCore;

public class ForumAppContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Subforum> Subforums => Set<Subforum>();

    public ForumAppContext()
    {
    }

    public ForumAppContext(DbContextOptions<ForumAppContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=forumapp;Username=forumapp;Password=forumapp_dev");
        }
    }

    /*
    Databaseforbindelsen kunne også være konfigureret via Dependency Injection
    i WebAPI's Program.cs ved at bruge AddDbContext().
    */

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Content>().HasKey(c => c.ContentId);
    }
    // EF Core kunne ikke genkende en PK for Comment og Post under initial migration

    /*
    TPH - Table-per-Hierarchy
    TPT - Table-per-Type
    */

}