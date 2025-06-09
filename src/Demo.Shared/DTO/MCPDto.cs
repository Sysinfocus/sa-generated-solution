namespace Demo.Shared.DTO;
public sealed class MCPDto : ModelValidator
{
    public Guid Id { get; set; } // NoAdd NoUpdate NoList
    public string Name { get; set; } = string.Empty;
    public string? Overview { get; set; } // Textarea
    public bool OfficialVendor { get; set; } // Switch

    public override Dictionary<string, string>? Errors()
    {
        var errors = new Dictionary<string, string>();
        if (!Name.IsLength(5, 100)) errors.TryAdd("Name", "Name must be between 5 and 100 chars.");
        return errors.Count == 0 ? null : errors;
    }
}
