using ApiContracts;
using EfcRepositories;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace WebAPI;

public static class ForumAppSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var ctx = scope.ServiceProvider
            .GetRequiredService<ForumAppContext>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();

        // Avoid inserting seed data more than once.
        if (await ctx.Users.AnyAsync())
            return;

        // USERS
        var admin = CreateUser(
            "admin", "ADMIN123!", UserRole.Admin, passwordHasher);

        var alice = CreateUser(
            "alice", "ALICE123!", UserRole.User, passwordHasher);

        var bob = CreateUser(
            "bob", "BOB123!", UserRole.User, passwordHasher);

        var charlie = CreateUser(
            "charlie", "CHARLIE123!", UserRole.User, passwordHasher);

        var julie = CreateUser(
            "julie", "JULIE123!", UserRole.User, passwordHasher);

        ctx.Users.AddRange(admin, alice, bob, charlie, julie);
        await ctx.SaveChangesAsync();

        // SUBFORUMS
        var general = new Subforum
        {
            Name = "General Discussion",
            UserId = admin.UserId,
            User = admin
        };

        var programming = new Subforum
        {
            Name = "Programming",
            UserId = alice.UserId,
            User = alice
        };

        var study = new Subforum
        {
            Name = "Study and Learning",
            UserId = bob.UserId,
            User = bob
        };

        ctx.Subforums.AddRange(general, programming, study);
        await ctx.SaveChangesAsync();

        // POSTS
        var post1 = new Post
        {
            Title = "Welcome to the forum!",
            Body = "Welcome everyone! Feel free to introduce yourselves.",
            User = admin,
            Subforum = general,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-7)
        };

        var post2 = new Post
        {
            Title = "What are you currently working on?",
            Body = "Share your current projects and what technologies you are using.",
            User = alice,
            Subforum = general,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-5)
        };

        var post3 = new Post
        {
            Title = "Tips for learning C#",
            Body = "What are your favourite resources for learning C# and .NET?",
            User = bob,
            Subforum = programming,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-4)
        };

        var post4 = new Post
        {
            Title = "Understanding Entity Framework",
            Body = "I am trying to understand how Entity Framework handles relationships. Any good explanations?",
            User = charlie,
            Subforum = programming,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-3)
        };

        var post5 = new Post
        {
            Title = "How do you prepare for exams?",
            Body = "I would love to hear how other students organize their study time.",
            User = julie,
            Subforum = study,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-2)
        };

        var post6 = new Post
        {
            Title = "Useful resources for learning programming",
            Body = "Let's collect some useful websites, books and other resources.",
            User = alice,
            Subforum = study,
            Timestamp = DateTimeOffset.UtcNow.AddDays(-1)
        };

        ctx.Posts.AddRange(post1, post2, post3, post4, post5, post6);
        await ctx.SaveChangesAsync();

        // TOP-LEVEL COMMENTS
        var comment1 = CreateComment(
            "Thanks for setting up the forum!",
            alice, post1);

        var comment2 = CreateComment(
            "Happy to be here!",
            bob, post1);

        var comment3 = CreateComment(
            "I am currently working on a Blazor application.",
            charlie, post2);

        var comment4 = CreateComment(
            "Microsoft Learn is a great place to start.",
            admin, post3);

        var comment5 = CreateComment(
            "I recommend experimenting with small projects.",
            julie, post3);

        var comment6 = CreateComment(
            "Have you looked into navigation properties?",
            alice, post4);

        var comment7 = CreateComment(
            "I usually make a study plan a week before the exam.",
            bob, post5);

        ctx.Comments.AddRange(
            comment1, comment2, comment3, comment4,
            comment5, comment6, comment7);

        await ctx.SaveChangesAsync();

        // REPLIES
        var reply1 = CreateComment(
            "Same here! Blazor is really interesting.",
            julie, post2, comment3);

        var reply2 = CreateComment(
            "That sounds useful. I will check it out.",
            charlie, post3, comment4);

        var reply3 = CreateComment(
            "Yes, that helped me understand the relationship.",
            bob, post4, comment6);

        ctx.Comments.AddRange(reply1, reply2, reply3);
        await ctx.SaveChangesAsync();

        // REACTIONS
        var reactions = new List<Reaction>
        {
            // Post 1
            CreateReaction(alice, post1.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(bob, post1.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(charlie, post1.ContentId, ContentType.Post, ReactionType.Like),

            // Post 2
            CreateReaction(admin, post2.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(julie, post2.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(bob, post2.ContentId, ContentType.Post, ReactionType.Dislike),

            // Post 3
            CreateReaction(alice, post3.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(charlie, post3.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(julie, post3.ContentId, ContentType.Post, ReactionType.Like),

            // Post 4
            CreateReaction(admin, post4.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(bob, post4.ContentId, ContentType.Post, ReactionType.Like),

            // Post 5
            CreateReaction(alice, post5.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(charlie, post5.ContentId, ContentType.Post, ReactionType.Dislike),

            // Post 6
            CreateReaction(admin, post6.ContentId, ContentType.Post, ReactionType.Like),
            CreateReaction(bob, post6.ContentId, ContentType.Post, ReactionType.Like),

            // Comments
            CreateReaction(admin, comment1.ContentId, ContentType.Comment, ReactionType.Like),
            CreateReaction(bob, comment1.ContentId, ContentType.Comment, ReactionType.Like),
            CreateReaction(alice, comment3.ContentId, ContentType.Comment, ReactionType.Like),
            CreateReaction(charlie, comment4.ContentId, ContentType.Comment, ReactionType.Like),
            CreateReaction(julie, comment6.ContentId, ContentType.Comment, ReactionType.Like)
        };

        ctx.Reactions.AddRange(reactions);
        await ctx.SaveChangesAsync();
    }

    private static User CreateUser(
        string username,
        string password,
        UserRole role,
        IPasswordHasher passwordHasher)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = string.Empty,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);

        return user;
    }

    private static Comment CreateComment(
        string body,
        User user,
        Post post,
        Comment? parentComment = null)
    {
        return new Comment
        {
            Body = body,
            User = user,
            Post = post,
            ParentComment = parentComment,
            Timestamp = DateTimeOffset.UtcNow.AddHours(-1)
        };
    }

    private static Reaction CreateReaction(
        User user,
        int contentId,
        ContentType contentType,
        ReactionType reactionType)
    {
        return new Reaction
        {
            User = user,
            ContentId = contentId,
            ContentType = contentType,
            ReactionType = reactionType
        };
    }
}