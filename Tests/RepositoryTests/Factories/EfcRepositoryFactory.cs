namespace RepositoryTests.Factories;

using EfcRepositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

public class EfcRepositoryFactory : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly ForumAppContext ctx;

    public IUserRepository UserRepository { get; }
    // public IPostRepository PostRepository { get; }
    // public ICommentRepository CommentRepository { get; }
    // public IReactionRepository ReactionRepository { get; }
    public ISubforumRepository SubforumRepository { get; }

    public EfcRepositoryFactory()
    {
        // Opretter en separat in-memory database
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        // Konfigurerer DbContext til at bruge testdatabasen
        var options = new DbContextOptionsBuilder<ForumAppContext>()
            .UseSqlite(connection)
            .Options;

        ctx = new ForumAppContext(options);

        // Opretter database og tabeller
        ctx.Database.EnsureCreated();

        // Opretter repositories
        UserRepository = new UserEfcRepository(ctx);
        // PostRepository = new PostEfcRepository(ctx);
        // CommentRepository = new CommentEfcRepository(ctx);
        // ReactionRepository = new ReactionEfcRepository(ctx);
        SubforumRepository = new SubforumEfcRepository(ctx);
    }

    public void Dispose()
    {
        ctx.Dispose();
        connection.Dispose();
    }
}