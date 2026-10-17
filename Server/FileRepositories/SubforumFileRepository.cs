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

    public async Task<bool> SubforumNameExistsAsync(string name)
    {
        List<Subforum> subforums = await GetManyAsync();

        return subforums.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

}