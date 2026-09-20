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
}