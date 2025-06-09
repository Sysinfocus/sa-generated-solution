namespace Demo.Shared.Utilities;

public sealed record PagedResult<T>(T? Data, int PageSize, int CurrentPage, int TotalRecords)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}