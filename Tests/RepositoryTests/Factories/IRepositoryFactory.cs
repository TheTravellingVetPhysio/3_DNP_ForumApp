using RepositoryContracts;

namespace RepositoryTests.Factories;

public interface IRepositoryFactory : IDisposable
{
    IUserRepository UserRepository { get; }
    IPostRepository PostRepository { get; }
    ICommentRepository CommentRepository { get; }
    IReactionRepository ReactionRepository { get; }
    ISubforumRepository SubforumRepository { get; }
}