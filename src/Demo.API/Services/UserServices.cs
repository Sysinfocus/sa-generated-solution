namespace Demo.API.Services;

public sealed class UserServices(AppDbContext context) : IServices<User, Guid>
{
    public async Task<User?> Create(User model, CancellationToken cancellationToken)
    {
        // If uniqueness is required, uncomment and validate accordingly.
        // var existing = await context.Users.FirstOrDefaultAsync(x => x.Name == model.Name, cancellationToken);
        // if (existing is not null) return default;
        context.Users.Add(model);
        await context.SaveChangesAsync(cancellationToken);
        return model;
    }

    public async Task<User?> Read(Guid id, CancellationToken cancellationToken)
        => await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User[]?> ReadAll(Func<User, bool>? predicate = null, CancellationToken cancellationToken = default)
    {
        var result = context.Users.AsNoTracking();
        if (predicate is not null) return Task.FromResult<User[]?>([.. result.Where(predicate)]);
        return Task.FromResult<User[]?>([.. result]);
    }

    public Task<PagedResult<User[]?>> ReadAll<T>(Func<User, bool>? predicate = null, int size = 0, int page = 0,
        bool isAscending = true, Func<User, T>? order = null, CancellationToken cancellationToken = default)
    {
        var result = context.Users.AsNoTracking().OrderBy(x => x.Id).AsQueryable();
        if (predicate is not null) result = result.Where(predicate).AsQueryable();
        
        if (isAscending && order is not null) result = result.OrderBy(order).AsQueryable();
        else if (!isAscending && order is not null) result = result.OrderByDescending(order).AsQueryable();
        
        int totalRecords = result.Count();
        if (size != 0 && page == 0) result = result.Take(size);
        else if (size != 0 && page != 0) result = result.Skip(size * (page - 1)).Take(size);
        var final = result.ToArray();
        return Task.FromResult(new PagedResult<User[]?>(final, size, page, totalRecords));
    }

    public async Task<bool> Update(Guid id, User model, CancellationToken cancellationToken)
    {
        var existing = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null) return false;
        context.ChangeTracker.Clear();
        var updated = model with { Id = existing.Id };
        context.Users.Update(updated);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existing = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null) return false;
        context.ChangeTracker.Clear();
        var delete = existing with { Id = id };
        context.Users.Remove(delete);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}