using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon;

public static class IEnumerableExtensions
{
    //public static string ToDelimitedString(this IEnumerable<string?> source, string separator = ", ")
    //{
    //    return string.Join(separator, source);
    //}

    public static string ToDelimitedString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        return string.Join(separator, source);
    }
}