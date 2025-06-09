namespace Demo.UI.Base;

public abstract class BaseUpdate<TPage, TModel, TKey> : ComponentBase where TModel : class, new()
{
    [Inject] internal BrowserExtensions BrowserExtensions { get; set; } = default!;
    [Inject] internal ApiService ApiService { get; set; } = default!;

    [CascadingParameter] internal BaseEndpoints Endpoints { get; set; } = default!;
    [Parameter] public TKey? Id { get; set; }
    [Parameter] public EventCallback<bool> OnClose { get; set; }

    public TModel? Model;
    public string Title = string.Empty;
    public bool Show = true;
    public bool IsBusy;
    public Dictionary<string, string>? Errors;
    public string? ErrorMessage, ImageError;

    //Methods for fetching data
    public async Task LoadData()
    {
        ErrorMessage = null;
        Model = null;
        if (Endpoints is null) throw new ArgumentNullException(nameof(Endpoints));
        IsBusy = true;
        var apiResult = await ApiService.Get<TModel?>(string.Format(Endpoints.ReadById, Id));
        if (apiResult.IsSuccess)
            Model = apiResult.Value;            
        else
            ErrorMessage = apiResult.Message;
        IsBusy = false;
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            await InvokeAsync(async () =>
            {
                StateHasChanged();
                await Task.Delay(ApiService.TOAST_SHOW_SECONDS).ContinueWith(_ => ErrorMessage = null);
            });
        }
    }

    //Methods for updating

    protected abstract Task HandleUpdate();

    public async Task HandleDiscard()
    {
        Show = false;
        await OnClose.InvokeAsync(false);
    }

    public void ShowError(Dictionary<string, string> err)
    {
        ImageError = "";
        foreach (var e in err)
            ImageError += e.Value;
    }    
}
