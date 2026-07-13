using Ardalis.Result;
using Ardalis.Specification;

namespace BusStop.UseCases;

public static class RepositoryExtensions
{
    /// <summary>
    /// Used by query handlers (IQueryHandler). Query handlers only read, so they inject <see cref="IReadRepository{T}"/>.
    /// </summary>
    public static async Task<Result<T>> FindRequiredAsync<T>(
        this IReadRepository<T> repository,
        ISpecification<T> spec,
        string notFoundMessage,
        CancellationToken ct) where T : class, IAggregateRoot
    {
        var entity = await repository.FirstOrDefaultAsync(spec, ct);
        return entity is null
            ? Result<T>.NotFound(notFoundMessage)
            : Result<T>.Success(entity);
    }

    /// <summary>
    /// Used by command handlers (ICommandHandler). Command handlers inject <see cref="IRepository{T}"/> for writes,
    /// but also need entity lookups before mutating.
    /// </summary>
    public static async Task<Result<T>> FindRequiredAsync<T>(
        this IRepository<T> repository,
        ISpecification<T> spec,
        string notFoundMessage,
        CancellationToken ct) where T : class, IAggregateRoot
    {
        var entity = await repository.FirstOrDefaultAsync(spec, ct);
        return entity is null
            ? Result<T>.NotFound(notFoundMessage)
            : Result<T>.Success(entity);
    }
}
