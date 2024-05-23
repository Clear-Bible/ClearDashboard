using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace ClearDashboard.DAL.Alignment.Corpora;

public static class SplitInstructionsExtensions
{

    public static bool ValidateFirstAndLastIndexes(this List<int> splitIndexes, int surfaceTextLength, out string? message)
    {
       
        var firstIndexIsValid = true;
        var lastIndexIsValid = true;
        message = null;

        if (splitIndexes.Count == 0)
        {
            return true;
        }

        var builder = new StringBuilder();
        if (splitIndexes[0] < 1)
        {
            firstIndexIsValid = false;
            builder.AppendLine($"The first split index must be greater than or equal to 1. First split index: '{splitIndexes[0]}'");
        }

        if (splitIndexes[^1] >= surfaceTextLength)
        {
            lastIndexIsValid = false;
            builder.AppendLine($"The last split index must be less than the 'SurfaceText' length. Last split index: '{splitIndexes[^1]}'");
        }

        if (!firstIndexIsValid || !lastIndexIsValid)
        {
            message = builder.ToString();
        }

        return firstIndexIsValid && lastIndexIsValid;
    }

    public static bool ValidateFirstAndLastIndexes(this List<SplitInstruction> splitInstructions, int? surfaceTextLength, out string? message)
    {

        var firstIndexIsValid = true;
        var lastIndexIsValid = true;
        message = null;

        var builder = new StringBuilder();
        if (splitInstructions[0].Index < 0)
        {
            firstIndexIsValid = false;
            builder.AppendLine($"The first split index must be greater than or equal to 0. First split index: '{splitInstructions[0].Index}'");
        }

        if (splitInstructions[^1].Index >= surfaceTextLength)
        {
            lastIndexIsValid = false;
            builder.AppendLine($"The last split index must be less than the 'SurfaceText' length. Last split index: '{splitInstructions[^1].Index}'");
        }

        if (!firstIndexIsValid || !lastIndexIsValid)
        {
            message = builder.ToString();
        }

        return firstIndexIsValid && lastIndexIsValid;
    }


    public static bool ValidateIndexesInAscendingOrder(this List<int> splitIndexes, out string? message)
    {
        if (splitIndexes.Count == 0)
        {
            message = null;
            return true;
        }

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