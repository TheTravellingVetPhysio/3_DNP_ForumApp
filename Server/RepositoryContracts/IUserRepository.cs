using Entities;

namespace RepositoryContracts;

public interface IUserRepository : IRepository<User>
{
    Task<bool> UsernameExistsAsync(string username);
}
