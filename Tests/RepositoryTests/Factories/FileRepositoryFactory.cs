using FileRepositories;
using RepositoryContracts;

namespace RepositoryTests.Factories;

public class FileRepositoryFactory : IRepositoryFactory
{
    private readonly string dataDirectory;

    public IUserRepository UserRepository { get; }
    public IPostRepository PostRepository { get; }
    public ICommentRepository CommentRepository { get; }
    public IReactionRepository ReactionRepository { get; }
    public ISubforumRepository SubforumRepository { get; }

    public FileRepositoryFactory()
    {
        dataDirectory = Path.Combine(
            Path.GetTempPath(),
            $"RepositoryTests_{Guid.NewGuid()}");

        Directory.CreateDirectory(dataDirectory);

        // Repositories without dependencies
        UserRepository = new UserFileRepository(dataDirectory);
        ReactionRepository = new ReactionFileRepository(dataDirectory);

        // Repositories with dependencies
        CommentRepository = new CommentFileRepository(
            UserRepository,
            ReactionRepository,
            dataDirectory);

        PostRepository = new PostFileRepository(
            UserRepository,
            CommentRepository,
            ReactionRepository,
            dataDirectory);

        SubforumRepository = new SubforumFileRepository(
            PostRepository,
            CommentRepository,
            dataDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(dataDirectory))
        {
            Directory.Delete(dataDirectory, recursive: true);
        }
    }
}