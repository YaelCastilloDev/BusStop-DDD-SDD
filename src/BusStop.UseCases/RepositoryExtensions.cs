using Ardalis.Result;
using Ardalis.Specification;

namespace BusStop.UseCases;

public static class RepositoryExtensions
{
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
