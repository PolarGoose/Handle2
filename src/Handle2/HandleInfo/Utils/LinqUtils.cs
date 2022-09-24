namespace Handle2.HandleInfo.Utils;

internal static class LinqUtils
{
    public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> grouping, out TKey key, out IEnumerable<TElement> elements)
    {
        key = grouping.Key;
        elements = grouping;
    }
}
