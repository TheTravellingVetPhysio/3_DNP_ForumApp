namespace FileRepositories;

using Entities;
using RepositoryContracts;

public class SubforumFileRepository : FileRepository<Subforum>, ISubforumRepository
{
    public SubforumFileRepository() : base(
        filePath: "Data/subforums.json",
        getId: s => s.SubforumId,
        setId: (s, id) => s.SubforumId = id)
    {
    }
}