using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Extensions;

public static class SplitInstructionsExtensions
{
    public static bool ValidateIndexesInAscendingOrder(this List<int> splitIndexes, out string? message)
    {
        var valid = splitIndexes.IsOrderedUsingSpanSort(out message, Comparer<int>.Create((a, b) => a.CompareTo(b)));
        if (!valid)
        {
            var builder = new StringBuilder(message);
            builder.AppendLine();
            for (var i = 0; i < splitIndexes.Count; i++)
            {
                builder.AppendLine($"Index: {i}, Value: {splitIndexes[i]}");
            }
            message = builder.ToString();
        }

        return valid;
    }

    public static bool ValidateIndexesInAscendingOrder(this List<SplitInstruction> splitIndexes, out string? message)
    {
        var valid = splitIndexes.IsOrderedUsingSpanSort(out message, Comparer<SplitInstruction>.Create((a, b) => a.Index.CompareTo(b.Index)));
        if (!valid)
        {
            var builder = new StringBuilder(message);
            builder.AppendLine();
            for (var i = 0; i < splitIndexes.Count; i++)
            {
                builder.AppendLine($"Index: {i}, Value: {splitIndexes[i]}");
            }
            message = builder.ToString();
        }

        return valid;
    }

    public static bool ValidateSurfaceText(this List<SplitInstruction> splitInstructions, string? surfaceText, out string? message)
    {
        message = null;
        var tokenTexts = splitInstructions.Select(i => i.TokenText);
        var combinedTokenTexts = string.Join(string.Empty, tokenTexts);
        var valid = combinedTokenTexts == surfaceText;

        if (!valid)
        {
            message = $"SurfaceText: '{surfaceText}', aggregated token text: '{combinedTokenTexts}'";
        }

        return valid;
    }

    public static bool ValidateTokenLengths(this List<SplitInstruction> splitInstructions, out string? message)
    {
        message = null;
        var builder = new StringBuilder("Invalid token lengths: ");
        bool isValid = true;
        foreach (var splitInstruction in splitInstructions)
        {
            if (splitInstruction.Length != splitInstruction.TokenText.Length)
            {
                builder.AppendLine();
                builder.AppendLine($"\t TokenText: '{splitInstruction.TokenText}', TokenText length: '{splitInstruction.TokenText.Length}', Length: '{splitInstruction.Length}'");
                isValid = false;
            }
        }

        message = isValid ? null : builder.ToString();
        return isValid;
    }



    public static bool IsOrderedUsingSpanSort<T>(this List<T> list, out string? message, IComparer<T>? comparer = default)
    {
        message = null;
        if (list.Count <= 1)
        {
            return true;
        }

        T[]? array = null;
        try
        {
            comparer ??= Comparer<T>.Default;
            var listSpan = CollectionsMarshal.AsSpan(list);
            var length = listSpan.Length;
            array = ArrayPool<T>.Shared.Rent(length);
            var arraySpan = array.AsSpan(0, length);
            listSpan.CopyTo(arraySpan);
            arraySpan.Sort(comparer);
            for (var i = 0; i < length; i++)
            {
                if (comparer.Compare(listSpan[i], arraySpan[i]) != 0)
                {
                    var comparedType = typeof(T);

                    var value = 0;
                    if (comparedType == typeof(int))
                    {
                        value = (int)(object)listSpan[i];
                    }

                    if (comparedType == typeof(SplitInstruction))
                    {
                        value = ((SplitInstruction)(object)listSpan[i]).Index;
                    }
                    message = $"The list is not ordered in ascending order at list index: '{i}', index value: '{value}'.";
                    return false;
                }
            }
            return true;
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<T>.Shared.Return(array);
            }
        }
    }
}