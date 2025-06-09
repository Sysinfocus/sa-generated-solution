namespace Demo.UI.Services;

public sealed class LoginRequestDto : ModelValidator
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }

    public override Dictionary<string, string>? Errors()
    {
        var errors = new Dictionary<string, string>();

        if (!Email.IsEmail()) errors.TryAdd(nameof(Email), $"{nameof(Email)} is required.");
        if (string.IsNullOrWhiteSpace(Password)) errors.TryAdd(nameof(Password), $"{nameof(Password)} is required.");
        else if (!Password.IsLength(6, 20)) errors.TryAdd(nameof(Password), $"{nameof(Password)} must be between 6 and 20 chars only.");

        return errors?.Count == 0 ? null : errors;
    }
}

public sealed class RegisterRequestDto : ModelValidator
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }

    public override Dictionary<string, string>? Errors()
    {
        var errors = new Dictionary<string, string>();

        if (!Email.IsEmail()) errors.TryAdd(nameof(Email), $"{nameof(Email)} is required.");
        if (string.IsNullOrWhiteSpace(Password)) errors.TryAdd(nameof(Password), $"{nameof(Password)} is required.");
        else if (!Password.IsLength(6, 20)) errors.TryAdd(nameof(Password), $"{nameof(Password)} must be between 6 and 20 chars only.");
        if (string.IsNullOrWhiteSpace(ConfirmPassword)) errors.TryAdd(nameof(ConfirmPassword), $"Confirm Password is required.");
        else if (Password != ConfirmPassword) errors.TryAdd(nameof(ConfirmPassword), $"Confirm Password to proceed.");

        return errors?.Count == 0 ? null : errors;
    }
}

public sealed record UserAuthentication(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);