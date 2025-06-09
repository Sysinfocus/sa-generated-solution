namespace Demo.UI.Base;

public abstract class BaseListing<TPage, TModel> : ComponentBase
{
    [Inject] internal BrowserExtensions BrowserExtensions { get; set; } = default!;
    [Inject] internal ApiService ApiService { get; set; } = default!;

    public string ModelName = typeof(TModel).Name.Replace("Dto", "");
    public string Title = "Page Title";
    public string Description = "This line shows some description about this page.";
    public string Url { get; set; } = string.Empty;
    public TModel[]? AllModels;
    public IEnumerable<TModel>? Models;
    public TModel? ActiveModel;
    public PaginationState Paging = new();
    public SortModel Sorting = new();

    public string? SearchFor, ErrorMessage;
    public bool ShowSearchBox, IsBusy, IsAdding, IsEditing, IsDeleting;
    protected BaseEndpoints? Endpoints;

    private BaseEndpoints Initialize()
    {
        Paging = new() { CurrentPage = 1, TotalRecords = 0, PageSize = 25 };
        Sorting = new() { Header = " ", IsAscending = true };
        return new BaseEndpoints()
        {
            Create = $"{Url}",
            Read = $"{Url}",
            ReadById = $"{Url}/{{0}}", // id
            Update = $"{Url}/{{0}}", // id
            Delete = $"{Url}/{{0}}", // id
        };
    }

    //Methods for fetching data
    public async Task LoadData()
    {
        Endpoints ??= Initialize();
        Models = null;
        ErrorMessage = null;
        if (Endpoints is null) throw new ArgumentNullException(nameof(Endpoints));
        IsBusy = true;
        var apiResult = await ApiService.Get<TModel[]?>(string.Format(Endpoints.Read, SearchFor));
        if (apiResult.IsSuccess)
        {
            AllModels = apiResult.Value;
            Models = AllModels.UpdatePaging(Paging);
        }
        else
        {
            ErrorMessage = apiResult.Message;
            Models = [];
        }
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

    //Methods for search
    public async Task HandleShowSearchBox()
    {
        ShowSearchBox = true;
        await BrowserExtensions.SetFocus("#searchBox");
    }

    protected abstract IEnumerable<TModel>? SearchResults();

    public void HandleSearch()
    {
        Models = SearchResults();
        Models.ResetPaging(Paging);
        HandlePaging();
    }

    //Methods for sorting
    protected abstract TModel[]? Sort(SortModel sortModel);
    public void HandleSorting(SortModel sortModel)
    {
        if (AllModels is null) return;
        AllModels = Sort(sortModel);
        Paging.CurrentPage = 1;
        HandlePaging();
    }

    //Methods for paging
    public void HandlePaging()
        => Models = SearchResults().PageData(Paging);

    //Methods for adding
    public void HandleAddNew()
    {
        IsAdding = true;
        ActiveModel = default;
    }

    public async Task HandleCloseAndRefresh(bool refresh)
    {
        IsAdding = false;
        IsEditing = false;
        if (!refresh) return;
        await LoadData();
    }


    //Methods for updates
    public void HandleEdit(TModel modelToUpdate)
    {
        ActiveModel = modelToUpdate;
        IsEditing = true;
    }


    //Methods for delete
    public void HandleConfirmDelete(TModel modelToDelete)
    {
        ActiveModel = modelToDelete;
        IsDeleting = true;
    }

    public void HandleDeleteCancelled()
    {
        ActiveModel = default;
        IsDeleting = false;
    }

    public async Task HandleDeleteConfirmed(string id)
    {
        HandleDeleteCancelled();
        if (Endpoints is null) throw new ArgumentNullException(nameof(Endpoints));

        IsBusy = true;
        var apiResult = await ApiService.Delete(string.Format(Endpoints.Delete, id));
        if (apiResult.IsSuccess)
            await HandleCloseAndRefresh(true);
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
}
