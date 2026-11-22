using RepositoryTests.Factories;
using Xunit;
using Entities;
using Exceptions;
using RepositoryContracts;

namespace RepositoryTests.Contracts;

public class UserRepositoryTests : IDisposable
{
    private readonly IRepositoryFactory factory;
    private readonly IUserRepository repository;

    public UserRepositoryTests()
    {
        factory = new EfcRepositoryFactory();
        repository = factory.UserRepository;
    }

    public void Dispose()
    {
        factory.Dispose();
    }

    // CRUD
    [Fact]
    public async Task AddAsync_ShouldAssignId_AndAllowRetrieval()
    {
        var user = new User
        {
            Username = "testuser",
            PasswordHash = "testhash"
        };

        var addedUser = await repository.AddAsync(user);

        Assert.True(addedUser.UserId > 0);

        var retrievedUser = await repository.GetSingleAsync(addedUser.UserId);

        Assert.Equal("testuser", retrievedUser.Username);
        Assert.Equal("testhash", retrievedUser.PasswordHash);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds_ToMultipleUsers()
    {
        var user1 = new User { Username = "user1", PasswordHash = "hash1" };
        var user2 = new User { Username = "user2", PasswordHash = "hash2" };

        var addedUser1 = await repository.AddAsync(user1);
        var addedUser2 = await repository.AddAsync(user2);

        Assert.NotEqual(addedUser1.UserId, addedUser2.UserId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdAtOne_WhenRepositoryIsEmpty()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "testhash"
        });

        Assert.Equal(1, user.UserId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenIdIsZero()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(0));
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenIdIsNegative()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(-1));
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnCorrectUser_WhenMultipleUsersExist()
    {
        await repository.AddAsync(new User
        {
            Username = "user1",
            PasswordHash = "hash1"
        });

        var user2 = await repository.AddAsync(new User
        {
            Username = "user2",
            PasswordHash = "hash2"
        });

        await repository.AddAsync(new User
        {
            Username = "user3",
            PasswordHash = "hash3"
        });

        var retrievedUser = await repository.GetSingleAsync(user2.UserId);

        Assert.Equal(user2.UserId, retrievedUser.UserId);
        Assert.Equal("user2", retrievedUser.Username);
        Assert.Equal("hash2", retrievedUser.PasswordHash);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllUsers()
    {
        await repository.AddAsync(new User { Username = "user1", PasswordHash = "hash1" });
        await repository.AddAsync(new User { Username = "user2", PasswordHash = "hash2" });

        var users = await repository.GetManyAsync();

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        var users = await repository.GetManyAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingUser()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "oldname",
            PasswordHash = "oldhash"
        });

        user.Username = "newname";
        user.PasswordHash = "newhash";

        await repository.UpdateAsync(user);

        var updatedUser = await repository.GetSingleAsync(user.UserId);

        Assert.Equal("newname", updatedUser.Username);
        Assert.Equal("newhash", updatedUser.PasswordHash);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotModifyOtherUsers()
    {
        var user1 = await repository.AddAsync(new User
        {
            Username = "user1",
            PasswordHash = "hash1"
        });

        var user2 = await repository.AddAsync(new User
        {
            Username = "user2",
            PasswordHash = "hash2"
        });

        user1.Username = "updateduser";
        await repository.UpdateAsync(user1);

        var unchangedUser = await repository.GetSingleAsync(user2.UserId);

        Assert.Equal("user2", unchangedUser.Username);
        Assert.Equal("hash2", unchangedUser.PasswordHash);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var user = new User
        {
            UserId = 999,
            Username = "unknown",
            PasswordHash = "hash"
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.UpdateAsync(user));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveUserId()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "testhash"
        });

        int originalId = user.UserId;

        user.Username = "updateduser";

        await repository.UpdateAsync(user);

        var updatedUser = await repository.GetSingleAsync(originalId);

        Assert.Equal(originalId, updatedUser.UserId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveExistingUser()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "testhash"
        });

        await repository.DeleteAsync(user.UserId);

        var users = await repository.GetManyAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherUsers()
    {
        var user1 = await repository.AddAsync(new User
        {
            Username = "user1",
            PasswordHash = "hash1"
        });

        var user2 = await repository.AddAsync(new User
        {
            Username = "user2",
            PasswordHash = "hash2"
        });

        await repository.DeleteAsync(user1.UserId);

        var remainingUser = await repository.GetSingleAsync(user2.UserId);

        Assert.Equal("user2", remainingUser.Username);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakeUserUnavailable()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "testhash"
        });

        await repository.DeleteAsync(user.UserId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(user.UserId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSameUserTwice()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "testhash"
        });

        await repository.DeleteAsync(user.UserId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(user.UserId));
    }

    // EKSTRA METODER FOR USERS
    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnTrue_WhenUsernameExists()
    {
        await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        bool exists = await repository.UsernameExistsAsync("testuser");

        Assert.True(exists);
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalse_WhenUsernameDoesNotExist()
    {
        bool exists = await repository.UsernameExistsAsync("unknown");

        Assert.False(exists);
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldIgnoreCase()
    {
        await repository.AddAsync(new User
        {
            Username = "TestUser",
            PasswordHash = "hash"
        });

        bool exists = await repository.UsernameExistsAsync("testuser");

        Assert.True(exists);
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalse_WhenRepositoryIsEmpty()
    {
        bool exists = await repository.UsernameExistsAsync("testuser");

        Assert.False(exists);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers_WhenUsernameIsNull()
    {
        await repository.AddAsync(new User { Username = "user1", PasswordHash = "hash1" });
        await repository.AddAsync(new User { Username = "user2", PasswordHash = "hash2" });

        var users = await repository.GetUsersAsync(null);

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers_WhenUsernameIsEmpty()
    {
        await repository.AddAsync(new User { Username = "user1", PasswordHash = "hash1" });
        await repository.AddAsync(new User { Username = "user2", PasswordHash = "hash2" });

        var users = await repository.GetUsersAsync("");

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers_WhenUsernameIsWhitespace()
    {
        await repository.AddAsync(new User { Username = "user1", PasswordHash = "hash1" });
        await repository.AddAsync(new User { Username = "user2", PasswordHash = "hash2" });

        var users = await repository.GetUsersAsync("   ");

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldFilterByUsername()
    {
        await repository.AddAsync(new User { Username = "testuser", PasswordHash = "hash1" });
        await repository.AddAsync(new User { Username = "anotheruser", PasswordHash = "hash2" });
        await repository.AddAsync(new User { Username = "mytestaccount", PasswordHash = "hash3" });

        var users = await repository.GetUsersAsync("test");

        Assert.Equal(2, users.Count);
        Assert.All(users, user => Assert.Contains("test", user.Username, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetUsersAsync_ShouldIgnoreCase()
    {
        await repository.AddAsync(new User { Username = "TestUser", PasswordHash = "hash" });

        var users = await repository.GetUsersAsync("testuser");

        Assert.Single(users);
        Assert.Equal("TestUser", users[0].Username);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnEmptyList_WhenNoUsersMatch()
    {
        await repository.AddAsync(new User { Username = "testuser", PasswordHash = "hash" });

        var users = await repository.GetUsersAsync("unknown");

        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenUsernameExists()
    {
        var user = await repository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var retrievedUser = await repository.GetUserByUsernameAsync("testuser");

        Assert.NotNull(retrievedUser);
        Assert.Equal(user.UserId, retrievedUser.UserId);
        Assert.Equal("testuser", retrievedUser.Username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
    {
        var user = await repository.GetUserByUsernameAsync("unknown");

        Assert.Null(user);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldIgnoreCase()
    {
        await repository.AddAsync(new User
        {
            Username = "TestUser",
            PasswordHash = "hash"
        });

        var user = await repository.GetUserByUsernameAsync("testuser");

        Assert.NotNull(user);
        Assert.Equal("TestUser", user.Username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenRepositoryIsEmpty()
    {
        var user = await repository.GetUserByUsernameAsync("testuser");

        Assert.Null(user);
    }
}