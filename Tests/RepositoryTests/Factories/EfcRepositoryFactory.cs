namespace RepositoryTests.Factories;

using EfcRepositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

public class EfcRepositoryFactory : IRepositoryFactory
{
    private readonly ForumAppContext ctx;

    public IUserRepository UserRepository { get; }
    public IPostRepository PostRepository { get; }
    public ICommentRepository CommentRepository { get; }
    public IReactionRepository ReactionRepository { get; }
    public ISubforumRepository SubforumRepository { get; }

    public EfcRepositoryFactory()
    {
        var options = new DbContextOptionsBuilder<ForumAppContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=forumapp_test;Username=forumapp;Password=forumapp_dev")
            .Options;

        ctx = new ForumAppContext(options);

        // Ensure that the database schema is up to date.
        ctx.Database.Migrate();

        // Clear all data and reset identity sequences.
        ctx.Database.ExecuteSqlRaw("""
            TRUNCATE TABLE
                "Content",
                "Reactions",
                "Subforums",
                "Users"
            RESTART IDENTITY CASCADE;
            """);

        UserRepository = new UserEfcRepository(ctx);
        ReactionRepository = new ReactionEfcRepository(ctx);
        CommentRepository = new CommentEfcRepository(ctx, ReactionRepository);
        PostRepository = new PostEfcRepository(ctx, CommentRepository, ReactionRepository);
        SubforumRepository = new SubforumEfcRepository(ctx);
    }

    public void Dispose()
    {
        ctx.Dispose();
    }
}