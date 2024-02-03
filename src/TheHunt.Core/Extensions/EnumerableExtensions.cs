namespace TheHunt.Core.Extensions;

public static class EnumerableExtensions
{
    public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> items)
    {
        return items as IReadOnlyList<T> ?? items.ToList();
    }
}