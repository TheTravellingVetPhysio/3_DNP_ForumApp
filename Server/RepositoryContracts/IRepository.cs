namespace RepositoryContracts;

public interface IRepository<TEntity>
{
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(int id);
    Task<TEntity> GetSingleAsync(int id);
    IQueryable<TEntity> GetManyAsync();
}