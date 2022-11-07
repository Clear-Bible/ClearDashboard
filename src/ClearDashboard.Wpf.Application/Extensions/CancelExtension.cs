using System.Collections.Generic;
using System.Threading;

namespace ClearDashboard.Wpf.Application.Extensions;

static class CancelExtension
{
    public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> en, CancellationToken token)
    {
        foreach (var item in en)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}