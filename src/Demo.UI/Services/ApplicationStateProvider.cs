namespace Demo.UI.Services;

public class ApplicationStateProvider(BrowserExtensions browserExtensions) : AuthenticationStateProvider
{
    private ClaimsPrincipal _user = new(new ClaimsIdentity());
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _user = new(new ClaimsIdentity());
        var token = await GetToken();
        if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(_user);
        var identity = new ClaimsIdentity(token);
        _user = new ClaimsPrincipal(identity);
        foreach(var claim in _user.Claims)
            Console.WriteLine(claim.Type + " = " + claim.Value);
        return new AuthenticationState(_user);
    }    

    private async Task<string?> GetToken()
    {        
        var authData = await browserExtensions.GetFromLocalStorage("authToken");
        if (authData is null) return null;
        var auth = JsonSerializer.Deserialize<UserAuthentication>(authData);
        if (auth is null) return null;
        return auth.AccessToken;
    }

    public async Task<string?> GetRefreshToken()
    {
        var authData = await browserExtensions.GetFromLocalStorage("authToken");
        if (authData is null) return null;
        var auth = JsonSerializer.Deserialize<UserAuthentication>(authData);
        if (auth is null) return null;
        return auth.RefreshToken;
    }

    public async Task Login(UserAuthentication response)
    {
        var auth = JsonSerializer.Serialize(response);
        var token = response.AccessToken;
        await browserExtensions.SetToLocalStorage("authToken", auth);
        var identity = new ClaimsIdentity(token);
        _user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    public async Task Logout()
    {
        await browserExtensions.RemoveFromLocalStorage("authToken");
        _user = new(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }
}