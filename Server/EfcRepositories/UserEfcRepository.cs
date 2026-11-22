namespace EfcRepositories;

using RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Entities;

public class UserEfcRepository : EfcRepository<User>, IUserRepository
{
    private readonly ForumAppContext ctx;

    public UserEfcRepository(ForumAppContext ctx) : base(ctx)
    {
        this.ctx = ctx;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await ctx.Users.SingleOrDefaultAsync(u =>
            u.Username.ToLower() == username.ToLower());
    }

    public async Task<List<User>> GetUsersAsync(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<User>();

        return await ctx.Users
            .Where(u => u.Username.ToLower().Contains(username.ToLower()))
            .ToListAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await ctx.Users.AnyAsync(u =>
            u.Username.ToLower() == username.ToLower());
    }
}