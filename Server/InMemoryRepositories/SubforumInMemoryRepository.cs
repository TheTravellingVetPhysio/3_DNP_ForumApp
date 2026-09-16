using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class SubforumInMemoryRepository : ISubforumRepository
{
    private readonly List<Subforum> subforums = new();

    public Task<Subforum> AddAsync(Subforum subforum)
    {
        subforum.SubforumId = subforums.Any()
            ? subforums.Max(s => s.SubforumId) + 1
            : 1;
        subforums.Add(subforum);
        return Task.FromResult(subforum);
    }

    public Task UpdateAsync(Subforum subforum)
    {
        Subforum? existingSubforum = subforums.SingleOrDefault(s => s.SubforumId == subforum.SubforumId);
        if (existingSubforum is null)
        {
            throw new InvalidOperationException(
                $"Subforum with ID '{subforum.SubforumId}' not found");
        }

        subforums.Remove(existingSubforum);
        subforums.Add(subforum);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Subforum? subforumToRemove = subforums.SingleOrDefault(s => s.SubforumId == id);
        if (subforumToRemove is null)
        {
            throw new InvalidOperationException(
                $"Subforum with ID '{id}' not found");
        }

        subforums.Remove(subforumToRemove);
        return Task.CompletedTask;
    }

    public Task<Subforum> GetSingleAsync(int id)
    {
        Subforum? subforum = subforums.SingleOrDefault(s => s.SubforumId == id);
        if (subforum is null)
        {
            throw new InvalidOperationException(
                $"Subforum with ID '{id}' not found");
        }

        return Task.FromResult(subforum);
    }

    public IQueryable<Subforum> GetManyAsync()
    {
        return subforums.AsQueryable();
    }
}