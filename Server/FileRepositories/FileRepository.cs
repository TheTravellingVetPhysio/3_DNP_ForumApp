namespace FileRepositories;

using System.Text.Json;
using Exceptions;

public abstract class FileRepository<T>
{
    private readonly string filePath;
    private readonly Func<T, int> getId;
    private readonly Action<T, int> setId;

    protected FileRepository(string filePath, Func<T, int> getId, Action<T, int> setId)
    {
        this.filePath = filePath;
        this.getId = getId;
        this.setId = setId;

        string? directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<T>> LoadAsync()
    {
        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    private async Task SaveAsync(List<T> entities)
    {
        string json = JsonSerializer.Serialize(entities);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T> AddAsync(T entity)
    {
        List<T> entities = await LoadAsync();

        int newId = entities.Any() ? entities.Max(e => getId(e)) + 1 : 1;
        setId(entity, newId);
        entities.Add(entity);

        await SaveAsync(entities);
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        List<T> entities = await LoadAsync();

        T? existing = entities.SingleOrDefault(e => getId(e) == getId(entity));
        if (existing is null)
        {
            throw new ResourceNotFoundException($"{typeof(T).Name} with ID '{getId(entity)}' not found");
        }

        entities.Remove(existing);
        entities.Add(entity);

        await SaveAsync(entities);
    }

    public async Task DeleteAsync(int id)
    {
        List<T> entities = await LoadAsync();

        T? toRemove = entities.SingleOrDefault(e => getId(e) == id);
        if (toRemove is null)
        {
            throw new ResourceNotFoundException($"{typeof(T).Name} with ID '{id}' not found");
        }

        entities.Remove(toRemove);
        await SaveAsync(entities);
    }

    public async Task<T> GetSingleAsync(int id)
    {
        List<T> entities = await LoadAsync();

        T? entity = entities.SingleOrDefault(e => getId(e) == id);
        if (entity is null)
        {
            throw new ResourceNotFoundException($"{typeof(T).Name} with ID '{id}' not found");
        }

        return entity;
    }

    public Task<List<T>> GetManyAsync()
        => LoadAsync();
}