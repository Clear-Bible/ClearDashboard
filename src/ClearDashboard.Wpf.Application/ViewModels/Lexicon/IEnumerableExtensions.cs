using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    public static List<string> Unjoin(this string source, string separator = ",") 
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        return source!.Split(separator).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
    }

    public static void SetInternalProperty(this object obj, string propertyName, object value)
    {
        var propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            propertyInfo.SetValue(obj, value);
        }
    }

    //public static List<string> Unjoin(this string source, string separator = ", ")
    //{
    //    if (source == null)
    //    {
    //        throw new ArgumentNullException(nameof(source));
    //    }
    //    return source!.Split(separator).Select(x => x.Trim()).ToList();
    //}
}