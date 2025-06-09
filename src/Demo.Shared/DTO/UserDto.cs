namespace Demo.Shared.DTO;

public class UserDto : ModelValidator
{
    public Guid Id { get; set; }
    public string Fullname { get; set; } = string.Empty;
    
    public override Dictionary<string, string>? Errors()
    {
        var errors = new Dictionary<string, string>();
        
        return errors.Count > 0 ? errors : null;
    }
}