namespace Demo.UI.Services;

public class ApiService(BrowserExtensions browserExtensions, Settings settings, HttpClient httpClient, AuthenticationStateProvider asp)
{
    private readonly string? ApiUrl = settings.ApiUrl;
    private bool refreshingToken = false;
    public static int TOAST_SHOW_SECONDS = 5000;

    private async Task RefreshTokenForUser(CancellationToken cancellationToken)
    {
        var url = "/api/v1/Auth/refresh";
        var token = await ((ApplicationStateProvider)(asp)).GetRefreshToken();
        var body = new { refreshToken = token };
        var output = await Post<object, UserAuthentication>(GetUrl(url), body, cancellationToken);
        if (output.IsSuccess && output.Value is not null)
        {
            await ((ApplicationStateProvider)(asp)).Login(output.Value);
            refreshingToken = false;
            return;
        }
        await ((ApplicationStateProvider)(asp)).Logout();
        browserExtensions.Goto("Login");
    }

    public async Task<Results<T?>> Get<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            retry:
            var request = await SetAuthenticationHeaders(GetUrl(url), HttpMethod.Get);
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return await GetSuccessResponse<T?>(response, cancellationToken);
            var result = await GetFailureResponse<T?>(request, response, cancellationToken);
            if (result.IsSuccess) goto retry;
            return result;
        }
        catch (Exception ex)
        {
            return new Results<T?>(default, HttpStatusCode.InternalServerError) { Message = ex.Message };
        }
    }

    public async Task<Results<T?>> Post<T>(string url, T model, CancellationToken cancellationToken = default)
        => await Post<T, T>(url, model, cancellationToken);

    public async Task<Results<TOut?>> Post<TIn, TOut>(string url, TIn model, CancellationToken cancellationToken = default)
    {
        try
        {
            retry:
            var request = await SetAuthenticationHeaders(GetUrl(url), HttpMethod.Post, model);
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return await GetSuccessResponse<TOut?>(response, cancellationToken);
            var result = await GetFailureResponse<TOut?>(request, response, cancellationToken);
            if (result.IsSuccess) goto retry;
            return result;
        }
        catch (Exception ex)
        {
            return new Results<TOut?>(default, HttpStatusCode.InternalServerError) { Message = ex.Message };
        }
    }

    public async Task<Results<bool>> Put<T>(string url, T model, CancellationToken cancellationToken = default)
    {
        try
        {
            retry:
            var request = await SetAuthenticationHeaders(GetUrl(url), HttpMethod.Put, model);
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return new Results<bool>(true, response.StatusCode);
            var result = await GetFailureResponse<bool>(request, response, cancellationToken);
            if (result.IsSuccess) goto retry;
            return result;
        }
        catch (Exception ex)
        {
            return new Results<bool>(default, HttpStatusCode.InternalServerError) { Message = ex.Message };
        }
    }

    public async Task<Results<bool>> Delete(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            retry:
            var request = await SetAuthenticationHeaders(GetUrl(url), HttpMethod.Delete);
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return new Results<bool>(true, response.StatusCode);
            var result = await GetFailureResponse<bool>(request, response, cancellationToken);
            if (result.IsSuccess) goto retry;
            return result;
        }
        catch (Exception ex) {
            return new Results<bool>(default, HttpStatusCode.InternalServerError) { Message = ex.Message };
        }
    }

    private async Task<HttpRequestMessage> SetAuthenticationHeaders(string url, HttpMethod httpMethod, object? model = null)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = httpMethod,
        };
        if (model is not null)
        {
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            var json = JsonSerializer.Serialize(model);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        var authData = await browserExtensions.GetFromLocalStorage("authToken");
        if (authData is null) return request;
        var auth = JsonSerializer.Deserialize<UserAuthentication>(authData);
        if (auth is null) return request;
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {auth.AccessToken}");
        return request;
    }

    private string GetUrl(string url)
    {
        if (url.StartsWith("http")) return url;
        return ApiUrl?.TrimEnd('/') + '/' + url.TrimStart('/');
    }

    private static async Task<Results<T?>> GetSuccessResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (typeof(T).Name == "String" || typeof(T).Name == "Object")
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new Results<T?>(default, response.StatusCode) { Message = content };
        }
        else
        {
            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            return new Results<T?>(result, response.StatusCode);
        }
    }

    private async Task<Results<T?>> GetFailureResponse<T>(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!refreshingToken && response.StatusCode == HttpStatusCode.Unauthorized && request.Headers.Contains("Authorization"))
        {
            refreshingToken = true;
            await RefreshTokenForUser(cancellationToken);
        }
        else if (refreshingToken && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            browserExtensions.Goto("Login");
        }
        return new Results<T?>(default, response.StatusCode)
        {
            Message = (await response.Content.ReadAsStringAsync(cancellationToken)).TrimStart('"').TrimEnd('"')
        };
    }
}

public record Results<T>(T? Value, HttpStatusCode StatusCode = HttpStatusCode.InternalServerError)
{
    public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public string? Message { get; set; }
    public object? Error { get; set; }
}

public sealed record Settings(string ApiUrl);