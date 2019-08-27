using System.Collections.Generic;
using System.Linq;

namespace DaiDai.Miscs
{
    public static class Enumerables
    {
        public static IEnumerable<T> From<T>(params T[] data)
        {
            return data;
        }

        public static List<T> List<T>(params T[] data)
        {
            return data.ToList();
        }

        public static bool IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }
    }

    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return Enumerables.IsNullOrEmpty(collection);
        }

        public static bool AnyNull<T>(this IEnumerable<T> collection)
        {
            return collection.Any(it => it == null);
        }

        public static bool AnyNotNull<T>(this IEnumerable<T> collection)
        {
            return collection.Any(it => it != null);
        }

        public static bool AllNull<T>(this IEnumerable<T> collection)
        {
            return collection.All(it => it == null);
        }

        public static bool AllNotNull<T>(this IEnumerable<T> collection)
        {
            return collection.All(it => it != null);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> collection)
        {
            return collection.Where(it => it != null);
        }
    }
}