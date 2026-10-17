namespace FileRepositories;

using Entities;
using RepositoryContracts;

public class UserFileRepository : FileRepository<User>, IUserRepository
{
    public UserFileRepository() : base(
        filePath: "Data/users.json",
        getId: u => u.UserId,
        setId: (u, id) => u.UserId = id)
    {
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        List<User> users = await GetManyAsync();

        return users.Any(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

}