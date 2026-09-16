using RepositoryContracts;

namespace InMemoryRepositories;

public abstract class InMemoryRepository<T> : IRepository<T>
{
    protected readonly List<T> entities = new();
    private readonly Func<T, int> getId;
    private readonly Action<T, int> setId;

    protected InMemoryRepository(Func<T, int> getId, Action<T, int> setId)
    {
        this.getId = getId;
        this.setId = setId;
    }

    public Task<T> AddAsync(T entity)
    {
        int newId = entities.Any() ? entities.Max(e => getId(e)) + 1 : 1;
        setId(entity, newId);
        entities.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        T? existing = entities.SingleOrDefault(e => getId(e) == getId(entity));
        if (existing is null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} with ID '{getId(entity)}' not found");
        }
        entities.Remove(existing);
        entities.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        T? toRemove = entities.SingleOrDefault(e => getId(e) == id);
        if (toRemove is null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} with ID '{id}' not found");
        }
        entities.Remove(toRemove);
        return Task.CompletedTask;
    }

    public Task<T> GetSingleAsync(int id)
    {
        T? entity = entities.SingleOrDefault(e => getId(e) == id);
        if (entity is null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} with ID '{id}' not found");
        }
        return Task.FromResult(entity);
    }

    public IQueryable<T> GetManyAsync()
    {
        return entities.AsQueryable();
    }
}