namespace RepositoryContracts;

public interface IRepository<T>
{
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<T> GetSingleAsync(int id);
    IQueryable<T> GetManyAsync();
}