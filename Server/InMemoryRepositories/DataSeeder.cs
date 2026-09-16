using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public static class DataSeeder
{
    public static async Task SeedAsync(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IReactionRepository reactionRepository,
        ISubforumRepository subforumRepository)
    {
        // Users
        User alice = await userRepository.AddAsync(new User { Username = "alice", Password = "password123" });
        User bob = await userRepository.AddAsync(new User { Username = "bob", Password = "hunter2" });
        User charlie = await userRepository.AddAsync(new User { Username = "charlie", Password = "qwerty" });

        // Subforums
        Subforum generalForum = await subforumRepository.AddAsync(new Subforum
        {
            Name = "Generelt",
            UserId = alice.UserId
        });

        Subforum dotnetForum = await subforumRepository.AddAsync(new Subforum
        {
            Name = "Dotnet & C#",
            UserId = bob.UserId
        });

        // Posts
        Post welcomePost = await postRepository.AddAsync(new Post
        {
            Title = "Velkommen til ForumApp",
            Body = "Dette er det allerførste indlæg på forummet. Sig hej!",
            UserId = alice.UserId,
            SubforumId = generalForum.SubforumId
        });

        Post dotnetPost = await postRepository.AddAsync(new Post
        {
            Title = "Hvad synes I om .NET?",
            Body = "Jeg er lige startet med C# og .NET, og jeg er ret imponeret indtil videre.",
            UserId = bob.UserId,
            SubforumId = dotnetForum.SubforumId
        });

        Post csharpTipsPost = await postRepository.AddAsync(new Post
        {
            Title = "Mine bedste C# tips",
            Body = "Her er nogle ting, jeg gerne ville have vidst tidligere...",
            UserId = charlie.UserId,
            SubforumId = dotnetForum.SubforumId
        });

        // Comments
        Comment comment1 = await commentRepository.AddAsync(new Comment
        {
            Body = "Fedt initiativ, glæder mig til at være med!",
            UserId = bob.UserId,
            PostId = welcomePost.ContentId
        });

        Comment comment2 = await commentRepository.AddAsync(new Comment
        {
            Body = "Enig, .NET er blevet meget bedre de sidste par år.",
            UserId = charlie.UserId,
            PostId = dotnetPost.ContentId
        });

        // Et svar på en eksisterende kommentar (nested comment)
        await commentRepository.AddAsync(new Comment
        {
            Body = "Ja, specielt performance-forbedringerne er tydelige.",
            UserId = alice.UserId,
            PostId = dotnetPost.ContentId,
            ParentCommentId = comment2.ContentId
        });

        await commentRepository.AddAsync(new Comment
        {
            Body = "Tak for delingen, meget nyttigt!",
            UserId = alice.UserId,
            PostId = csharpTipsPost.ContentId
        });

        // Reactions
        await reactionRepository.AddAsync(new Reaction
        {
            ReactionType = ReactionType.Like,
            UserId = bob.UserId,
            ContentId = welcomePost.ContentId
        });

        await reactionRepository.AddAsync(new Reaction
        {
            ReactionType = ReactionType.Like,
            UserId = charlie.UserId,
            ContentId = welcomePost.ContentId
        });

        await reactionRepository.AddAsync(new Reaction
        {
            ReactionType = ReactionType.Dislike,
            UserId = alice.UserId,
            ContentId = comment1.ContentId
        });
    }
}