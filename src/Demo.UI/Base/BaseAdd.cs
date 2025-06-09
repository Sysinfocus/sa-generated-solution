namespace Demo.UI.Base;

public abstract class BaseAdd<TPage, TModel> : ComponentBase where TModel : class, new()
{
    [Inject] internal BrowserExtensions BrowserExtensions { get; set; } = default!;
    [Inject] internal ApiService ApiService { get; set; } = default!;

    [CascadingParameter] internal BaseEndpoints Endpoints { get; set; } = default!;
    [Parameter] public EventCallback<bool> OnClose { get; set; }

    public TModel Model = new();
    public string Title = string.Empty;
    public bool Show = true;
    public bool IsBusy, RefreshRequired;
    public Dictionary<string, string>? Errors;
    public string? ErrorMessage, ImageError;


    //Methods for adding

    protected abstract Task HandleAdd();

    public async Task HandleClose()
    {
        Show = false;
        await OnClose.InvokeAsync(RefreshRequired);
    }

    public void ShowError(Dictionary<string, string> err)
    {
        ImageError = "";
        foreach (var e in err)
            ImageError += e.Value;
    }
}