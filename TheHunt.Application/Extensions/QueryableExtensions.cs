using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Linq;

public static class QueryableExtensions
{
    public static IQueryable<T> Page<T>(this IQueryable<T> queryable, int index, int size) => queryable.Skip(index * size).Take(size);
}