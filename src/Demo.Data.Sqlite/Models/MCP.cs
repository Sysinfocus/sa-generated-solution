namespace Demo.Data.Sqlite.Models;

public sealed record MCP(
	Guid Id,
	string Name,
	string? Overview,
	bool OfficialVendor);
