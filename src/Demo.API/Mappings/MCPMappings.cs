namespace Demo.API.Mappings;
public static class MCPMappings
{
    public static MCPDto ToDTO(this MCP model)
    {
        return new() {
			Id = model.Id,
			Name = model.Name,
			Overview = model.Overview,
			OfficialVendor = model.OfficialVendor
        };
    }

    public static MCP ToModel(this MCPDto dto, Guid? id = null)
    {
        return new(
            id ?? new(),
			dto.Name,
			dto.Overview,
			dto.OfficialVendor
        );
    }
}
