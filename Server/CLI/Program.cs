using RepositoryContracts;
using CLI.UI;
using FileRepositories;
using Services;

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
SubforumService subforumService = new SubforumService(subforumRepository, userService, postRepository);
PostService postService = new PostService(postRepository, userService, subforumService, commentService, reactionService);

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