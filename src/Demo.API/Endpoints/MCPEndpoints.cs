namespace Demo.API.Endpoints;
public sealed class MCPEndpoints(
    ILogger<MCPEndpoints> logger,
    INotifications notifications,
    HybridCache cache,
    IServices<MCP,Guid> services) : IEndpoints
{
    private readonly HybridCacheEntryOptions hybridCacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(5),
    };
    private const string version = "/api/v1/";
    private const string keyName = "MCPServers";
    public void Register(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(version + keyName).RequireAuthorization();

        group.MapGet("/{id:Guid}", Get);
        group.MapGet("/{search?}", GetAll);
        group.MapGet("/{size:int}/{page:int}/{isAscending:bool}/{property?}/{search?}", GetPaged);
        group.MapPost("/", Create);
        group.MapPut("/{id:Guid}", Update);
        group.MapDelete("/{id:Guid}", Delete);
    }

    private async Task<IResult> Create(MCPDto request, CancellationToken cancellationToken)
    {
        if (!request.IsValid)
        {
            var errors = request.Errors()?.ToDictionary(x => x.Key, x => new[] { x.Value })!;
            return Results.ValidationProblem(errors, "Validation problem", statusCode: 404);
        }

        var model = request.ToModel();
        var result = await services.Create(model, cancellationToken);
        if (result is null)
        {
            var errors = new Dictionary<string, string[]>
            {
                // Add validations as required
            };
            logger.LogError("Validation error for request: {request}", request);
            return Results.ValidationProblem(errors, null, null, 400, "Validation error.");
        }
        await cache.RemoveByTagAsync(keyName, cancellationToken);
        await notifications.Notify(result, result.Id.ToString(), NotificationAction.Created);
        return Results.Created($"{version}{keyName}/{result.Id}", result.ToDTO());
    }

    private async Task<IResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await cache.GetOrCreateAsync(
            $"{keyName}-{id}",
            async ct => await services.Read(id, ct),
            hybridCacheEntryOptions,
            tags: [keyName],
            cancellationToken: cancellationToken
        );
        if (result is null)
        {
            await cache.RemoveAsync($"{keyName}-{id}", cancellationToken);
            logger.LogWarning("MCP Id: {id} was accessed which doesn't exist.", id);
            return Results.NotFound("MCP not found.");
        }
        logger.LogInformation("MCP Id: {id} was accessed.", id);
        await notifications.Notify(result, $"{id}", NotificationAction.Read);
        return Results.Ok(result);
    }

    private async Task<IResult> GetAll(string? search = null, CancellationToken cancellationToken = default)
    {
        var pagedKey = $"{nameof(MCP)}-{search}";
        var result = await cache.GetOrCreateAsync(
            $"{pagedKey}",
            async ct => await ReadRecords(search, ct),
            hybridCacheEntryOptions,
            tags: [keyName],
            cancellationToken: cancellationToken
        );

        if (result?.Length == 0)
        {
            logger.LogWarning("All MCPServers accessed but none exists.");
            return Results.NotFound("MCPServers not found.");
        }
        logger.LogInformation("All MCPServers accessed.");
        await notifications.Notify<MCP>(default, null, NotificationAction.Read);
        return Results.Ok(result);
    }
    
    private async Task<MCP[]?> ReadRecords(string? search, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(search)) return await services.ReadAll(null, ct);
        
        return await services.ReadAll((x =>
            			x.Id.ToString().Match(search) ||
			x.Name.ToString().Match(search) ||
			x.Overview.ToString().Match(search) ||
			x.OfficialVendor.ToString().Match(search) 
        ), ct);
    }

    private async Task<IResult> GetPaged(int size, int page, bool isAscending, string? property = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var pagedKey = $"{nameof(MCP)}-{size}-{page}-{isAscending}-{property}-{search}";

        var result = await cache.GetOrCreateAsync(
            $"{pagedKey}",
            async ct => await ReadPagedRecords(size, page, isAscending, property, search, ct),
            hybridCacheEntryOptions,
            tags: [keyName],
            cancellationToken: cancellationToken
        );

        if (result?.Data?.Length == 0)
        {
            logger.LogWarning("Paged MCPServers accessed but none exists.");
            return Results.NotFound("MCPServers not found.");
        }
        logger.LogInformation("Paged MCPServers accessed.");
        await notifications.Notify<PagedResult<MCP>>(default, null, NotificationAction.Read);
        return Results.Ok(result);
    }    

    private async Task<PagedResult<MCP[]?>> ReadPagedRecords(int size, int page, bool isAscending, string? property, string? search, CancellationToken ct)
    {
        Func<MCP, dynamic?>? orderProperty = property switch
        {
            			"Id" => x => x.Id,
			"Name" => x => x.Name,
			"Overview" => x => x.Overview,
			"OfficialVendor" => x => x.OfficialVendor,

            _ => null
        };

        if (string.IsNullOrWhiteSpace(search)) return await services.ReadAll(null, size, page, isAscending, orderProperty, ct);

        return await services.ReadAll((x =>
            			x.Id.ToString().Match(search) ||
			x.Name.ToString().Match(search) ||
			x.Overview.ToString().Match(search) ||
			x.OfficialVendor.ToString().Match(search) 
        ), size, page, isAscending, orderProperty, ct);
    }

    private async Task<IResult> Update(Guid id, MCPDto request, CancellationToken cancellationToken)
    {
        if (!request.IsValid)
        {
            var errors = request.Errors()?.ToDictionary(x => x.Key, x => new[] { x.Value })!;
            return Results.ValidationProblem(errors, "Validation problem", statusCode: 404);
        }
        var model = request.ToModel(id);
        var result = await services.Update(id, model, cancellationToken);
        if (!result)
        {
            logger.LogWarning("Update failed for MCP Id: {id}", id);
            return Results.BadRequest();
        }
        await cache.RemoveByTagAsync(keyName, cancellationToken);
        await notifications.Notify(model, $"{id}", NotificationAction.Updated);
        return Results.Accepted();
    }

    private async Task<IResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await services.Delete(id, cancellationToken);
        if (!result)
        {
            logger.LogWarning("Delete failed for MCP Id: {id}", id);
            return Results.BadRequest();
        }
        await cache.RemoveByTagAsync(keyName, cancellationToken);
        await notifications.Notify<MCP>(null, $"{id}", NotificationAction.Deleted);
        return Results.NoContent();
    }
}
