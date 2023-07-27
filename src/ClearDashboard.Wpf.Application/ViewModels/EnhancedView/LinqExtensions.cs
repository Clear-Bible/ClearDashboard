using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public static class LinqExtensions
{
    public static async Task<TResult[]> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return await Task.WhenAll(source.Select(selector));
    }
}