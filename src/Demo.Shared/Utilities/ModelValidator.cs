namespace Demo.Shared.Utilities;

public abstract class ModelValidator
{
    public abstract Dictionary<string, string>? Errors();
    public bool IsValid => Errors() is null || Errors()?.Count == 0;
}