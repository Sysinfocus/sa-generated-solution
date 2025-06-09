namespace Demo.UI.Services;

public static class UIExtensions
{    
    public static IEnumerable<T> UpdatePaging<T>(this ICollection<T>? _data, PaginationState paging)
    {
        if (_data is null) return [];
        paging.TotalRecords = _data.Count;
        return _data.Take(paging.PageSize);
    }

    public static IEnumerable<T>? PageData<T>(this IEnumerable<T>? source, PaginationState paging)
    {
        var skip = (paging.CurrentPage - 1) * paging.PageSize;
        var balance = paging.TotalRecords - skip;
        balance = balance.If(balance > paging.PageSize, paging.PageSize);
        return source?.Skip(skip).Take(balance);
    }

    public static string PagingDetails(this PaginationState paging, string? search = null)
    {
        var count = paging.CurrentPage * paging.PageSize;
        count = count.If(count > paging.TotalRecords, paging.TotalRecords);

        var searchType = string.IsNullOrWhiteSpace(search) ? "" :
            search[0] == '*' ? "ending with" :
            search[0] == '=' ? "equal to" :
            search[^1] == '*' ? "starting with" :
            "containing";

        var searchTerm = searchType switch
        {
            "ending with" or "equal to" => search?[1..],
            "starting with" => search?[0..^1],
            _ => search,
        };

        var searchText = search.If(!string.IsNullOrEmpty(search), $" for search {searchType} '{searchTerm}'");
        if (paging.TotalRecords == 0) return $"No records{searchText}.";
        var start = paging.PageSize * (paging.CurrentPage - 1);
        var end = start + paging.PageSize;
        end = end > paging.TotalRecords ? paging.TotalRecords : end;
        return $"Showing {start + 1} to {end} from {paging.TotalRecords} records{searchText}.";
    }

    public static void ResetPaging<T>(this IEnumerable<T>? source, PaginationState paging)
    {
        if (source is null) return;
        paging.TotalRecords = source.Count();
        paging.CurrentPage = 1;
    }

    public async static Task<string?> HandleFileUpload(this IReadOnlyList<IBrowserFile> files, string propertyName, Dictionary<string,string>? Errors)
    {
        long maxFileSize = 1024 * 250;
        Errors?.Remove(propertyName);
        var file = files[0];
        var fileExtension = Path.GetExtension(file.Name).ToLower();
        if (".jpg .jpeg .png".Contains(fileExtension) == false) Errors?.TryAdd(propertyName, "Image file can be either JPG, JPEG or PNG only.");
        else if (file.Size > maxFileSize) Errors?.TryAdd(propertyName, "Image file size must be less than or equal to 250KB.");
        if (Errors?.HasError(propertyName) is not null) return null;

        using var ms = new MemoryStream();
        await file.OpenReadStream(maxFileSize).CopyToAsync(ms);
        //Model.Image = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
    }

    private static string? HasError(this Dictionary<string, string> errors, string name)
    {
        if (errors.ContainsKey(name) == false) return null;
        return "error";
    }

    public static MarkupString? HasErrorFor(this Dictionary<string, string> errors, string name)
    {
        if (errors?.ContainsKey(name) == false) return null;
        return (MarkupString)$"<p class='error'>{errors?[name]}</p>";
    }
}
