namespace Demo.Shared.Utilities;

public static class CommonExtensions
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public static T If<T>(this T oldValue, bool condition, T newValue) => condition ? newValue : oldValue;    

    public static IEnumerable<T>? Search<T>(this IEnumerable<T>? source, string? search, Func<T, bool> conditions)
        => string.IsNullOrEmpty(search) ? [.. source!] : [.. source!.Where(conditions)];

    public static bool Match(this string? source, string? find)
    {
        if (string.IsNullOrWhiteSpace(find) || string.IsNullOrWhiteSpace(source)) return true;

        return find[0] == '*' ? source.EndsWith(find[1..], StringComparison.OrdinalIgnoreCase)
            : find[0] == '=' ? source.Equals(find[1..], StringComparison.OrdinalIgnoreCase)
            : find[^1] == '*' ? source.StartsWith(find[0..^1], StringComparison.OrdinalIgnoreCase)
            : source.Contains(find, StringComparison.OrdinalIgnoreCase);
    }

    public static T[]? SortUpdate<T,TKey>(this T[] source, Func<T, TKey> selector, bool isAscending)
        => isAscending ? [.. source.OrderBy(selector)] : [.. source.OrderByDescending(selector)];

    public static DateTime UtcToIst(this DateTime dateTime) => dateTime.AddMinutes(360);

    public static string UtcToIst(this DateTime dateTime, string format) => dateTime.AddMinutes(360).ToString(format);
}
