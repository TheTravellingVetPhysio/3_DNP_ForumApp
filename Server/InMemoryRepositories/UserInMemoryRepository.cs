using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository : InMemoryRepository<User>, IUserRepository
{
    public UserInMemoryRepository() : base(
        getId: u => u.UserId,
        setId: (u, id) => u.UserId = id)
    {
    }
}