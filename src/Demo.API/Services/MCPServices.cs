namespace Demo.API.Services;
public sealed class MCPServices(AppDbContext context) : IServices<MCP, Guid>
{
    public async Task<MCP?> Create(MCP model, CancellationToken cancellationToken)
    {
        // If uniqueness is required, uncomment and validate accordingly.
        // var existing = await context.MCPServers.FirstOrDefaultAsync(x => x.Name == model.Name, cancellationToken);
        // if (existing is not null) return default;
        context.MCPServers.Add(model);
        await context.SaveChangesAsync(cancellationToken);
        return model;
    }

    public async Task<MCP?> Read(Guid id, CancellationToken cancellationToken)
        => await context.MCPServers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<MCP[]?> ReadAll(Func<MCP, bool>? predicate = null, CancellationToken cancellationToken = default)
    {
        var result = context.MCPServers.AsNoTracking();
        if (predicate is not null) return Task.FromResult<MCP[]?>([.. result.Where(predicate)]);
        return Task.FromResult<MCP[]?>([.. result]);
    }

    public Task<PagedResult<MCP[]?>> ReadAll<T>(Func<MCP, bool>? predicate = null, int size = 0, int page = 0,
        bool isAscending = true, Func<MCP, T>? order = null, CancellationToken cancellationToken = default)
    {
        var result = context.MCPServers.AsNoTracking().OrderBy(x => x.Id).AsQueryable();
        if (predicate is not null) result = result.Where(predicate).AsQueryable();
        
        if (isAscending && order is not null) result = result.OrderBy(order).AsQueryable();
        else if (!isAscending && order is not null) result = result.OrderByDescending(order).AsQueryable();
        
        int totalRecords = result.Count();
        if (size != 0 && page == 0) result = result.Take(size);
        else if (size != 0 && page != 0) result = result.Skip(size * (page - 1)).Take(size);
        var final = result.ToArray();
        return Task.FromResult(new PagedResult<MCP[]?>(final, size, page, totalRecords));
    }

    public async Task<bool> Update(Guid id, MCP model, CancellationToken cancellationToken)
    {
        var existing = await context.MCPServers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null) return false;
        context.ChangeTracker.Clear();
        var updated = model with { Id = existing.Id };
        context.MCPServers.Update(updated);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existing = await context.MCPServers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null) return false;
        context.ChangeTracker.Clear();
        var delete = existing with { Id = id };
        context.MCPServers.Remove(delete);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
