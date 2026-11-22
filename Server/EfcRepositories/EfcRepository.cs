namespace EfcRepositories;

using Exceptions;
using RepositoryContracts;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public class EfcRepository<T> : IRepository<T> where T : class
{
    private readonly ForumAppContext ctx;

    public EfcRepository(ForumAppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<T> AddAsync(T entity)
    {
        await ctx.Set<T>().AddAsync(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var (predicate, keyValue) = CreatePrimaryKeyPredicate(null, id);

        T? existing = await ctx.Set<T>().SingleOrDefaultAsync(predicate);
        if (existing is null)
        {
            throw new ResourceNotFoundException($"{typeof(T).Name} with ID '{keyValue}' not found");
        }

        ctx.Set<T>().Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<List<T>> GetManyAsync()
    {
        return await ctx.Set<T>().ToListAsync();
    }

    public async Task<T> GetSingleAsync(int id)
    {
        var (predicate, keyValue) = CreatePrimaryKeyPredicate(null, id);

        T? existing = await ctx.Set<T>().SingleOrDefaultAsync(predicate);
        if (existing is null)
        {
            throw new ResourceNotFoundException($"{typeof(T).Name} with ID '{keyValue}' not found");
        }
        
        return existing;
    }

    public async Task UpdateAsync(T entity)
    {
        var (predicate, keyValue) = CreatePrimaryKeyPredicate(entity, null);

        // Selve metoden
        if (!await ctx.Set<T>().AnyAsync(predicate))
        {
            throw new ResourceNotFoundException($"{typeof(T).Name} with ID '{keyValue}' not found");
        }

        ctx.Set<T>().Update(entity);
        await ctx.SaveChangesAsync();
    }

    private (Expression<Func<T, bool>> Predicate, object? KeyValue) CreatePrimaryKeyPredicate(T? entity, int? id)    {
        // Finder navnet og værdien på den enkelte entitets PK
        var primaryKey = ctx.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
        var keyProperty = primaryKey?.Properties.Single();

        if (keyProperty is null || keyProperty.PropertyInfo is null)
            throw new InvalidOperationException($"Could not find primary key for {typeof(T).Name}");
        
        object? keyValue;

        if (entity is not null)
        {
            keyValue = keyProperty.PropertyInfo.GetValue(entity);
        }
        else if (id.HasValue)
        {
            keyValue = id.Value;
        }
        else
        {
            throw new ArgumentException("Either entity or id must be provided.");
        }

        // Dynamisk oprettelse af e => e.Id == entity.Id til AnyAsync() og SingleOrDefault()
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, keyProperty.Name);
        var value = Expression.Constant(keyValue, keyProperty.ClrType);
        var comparison = Expression.Equal(property, value);
        var predicate = Expression.Lambda<Func<T, bool>>(comparison, parameter);
        
        return (predicate, keyValue);
    }
}