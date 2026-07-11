namespace BusStop.UseCases;

public static class CreateEntityPipeline
{
    public static async Task<Result<TResponse>> CreateAsync<TEntity, TResponse>(
        this IRepository<TEntity> repository,
        Result<TEntity> factoryResult,
        Func<TEntity, TResponse> map,
        CancellationToken ct) where TEntity : class, IAggregateRoot
    {
        if (!factoryResult.IsSuccess)
            return Result<TResponse>.Error(new ErrorList(factoryResult.Errors));

        var entity = factoryResult.Value;
        var created = await repository.AddAsync(entity, ct);
        return Result<TResponse>.Success(map(created));
    }
}
