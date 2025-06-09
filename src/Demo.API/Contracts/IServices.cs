namespace Demo.API.Contracts;

public interface IServices<TModel,TKey>
{
    Task<TModel?> Create(TModel model, CancellationToken cancellationToken);
    Task<TModel?> Read(TKey id, CancellationToken cancellationToken);
    Task<TModel[]?> ReadAll(Func<TModel, bool>? predicate = null, CancellationToken cancellationToken = default);
    Task<PagedResult<TModel[]?>> ReadAll<T>(Func<TModel, bool>? predicate = null, int size = 0, int page = 0,
        bool isAscending = true, Func<TModel, T>? property = null, CancellationToken cancellationToken = default);
    Task<bool> Update(TKey id, TModel model, CancellationToken cancellationToken);
    Task<bool> Delete(TKey id, CancellationToken cancellationToken);
}