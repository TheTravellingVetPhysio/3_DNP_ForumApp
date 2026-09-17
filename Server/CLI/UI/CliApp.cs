namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IReactionRepository reactionRepository;
    private readonly ISubforumRepository subforumRepository;

    public CliApp(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IReactionRepository reactionRepository,
        ISubforumRepository subforumRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.reactionRepository = reactionRepository;
        this.subforumRepository = subforumRepository;
    }

    public async Task RunAsync()
    {
        
    }
}