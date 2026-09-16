using Entities;
using RepositoryContracts;
using InMemoryRepositories;
using CLI.UI;

Console.WriteLine("Starting CLI app...");

IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();
IReactionRepository reactionRepository = new ReactionInMemoryRepository();
ISubforumRepository subforumRepository = new SubforumInMemoryRepository();

await DataSeeder.SeedAsync(
    userRepository,
    postRepository,
    commentRepository,
    reactionRepository,
    subforumRepository);

CliApp cliApp = new CliApp(
    userRepository,
    postRepository,
    commentRepository,
    reactionRepository,
    subforumRepository);

await cliApp.RunAsync();