using Entities;
using RepositoryContracts;
using CLI.UI;
using FileRepositories;

// Repositories
IUserRepository userRepository = new UserFileRepository();
IPostRepository postRepository = new PostFileRepository();
ICommentRepository commentRepository = new CommentFileRepository();
IReactionRepository reactionRepository = new ReactionFileRepository();
ISubforumRepository subforumRepository = new SubforumFileRepository();

// Services – rækkefølgen her er bestemt af KONSTRUKTØR-afhængigheder, ikke af domænelogik
UserService userService = new UserService(userRepository);
ReactionService reactionService = new ReactionService(reactionRepository, userRepository, postRepository, commentRepository);
CommentService commentService = new CommentService(commentRepository, postRepository, userService, reactionService);
PostService postService = new PostService(postRepository, subforumRepository, userService, commentService, reactionService);
SubforumService subforumService = new SubforumService(subforumRepository, userService, postService); // SIDST, fordi den kræver postService

await DataSeeder.SeedAsync(
    userService,
    postService,
    commentService,
    reactionService,
    subforumService);

CliApp cliApp = new CliApp(
    userService,
    postService,
    commentService,
    reactionService,
    subforumService);

await cliApp.RunAsync();