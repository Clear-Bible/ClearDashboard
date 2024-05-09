using System.Text;
using System.Text.Json.Serialization;
using ClearDashboard.DAL.Alignment.Exceptions;

namespace ClearDashboard.DAL.Alignment.Corpora;
public class SplitInstructions
{

    [JsonIgnore]
    public int Count => Instructions.Count;

    [JsonIgnore]
    public int? SurfaceTextLength => SurfaceText?.Length;

    [JsonIgnore]
    public string? ErrorMessage { get; private set; }

    public string? SurfaceText { get; set; }

    public List<SplitInstruction> Instructions { get; set; } = [];


    public SplitInstruction this[int index]
    {
        get => Instructions[index];
        set => Instructions[index] = value;
    }


    public static SplitInstructions CreateSplits(string surfaceText, List<int> splitIndexes, List<string?> trainingTexts)
    {
        if (splitIndexes.Count  != trainingTexts.Count - 1)
        {
            throw new SplitInstructionException(SplitInstructionErrorMessages.SplitIndexesMustBeOneLessThanTrainingTexts, $"'splitIndexes' count: {splitIndexes.Count}, 'trainingTexts' count: {trainingTexts.Count}");
        }

        if (!splitIndexes.ValidateIndexesInAscendingOrder(out string? message))
        {
            throw new SplitInstructionException($"The split indexes must be in ascending order.", message);
        }

        var splitInstructions = new SplitInstructions
        {
            SurfaceText = surfaceText
        };

        var index = 0;
        var surfaceTextLength = surfaceText.Length;
        foreach (var splitIndex in splitIndexes)
        {
            var last = splitIndexes.Last() == splitIndex;
            var first = splitIndexes.First() == splitIndex;
               
            string tokenText;

            if (first)
            {
                tokenText = surfaceText.Substring(0, splitIndex);
                splitInstructions.Instructions.Add(new SplitInstruction(0, tokenText, trainingTexts[index]));
                index++;
            }

            var length = last ? surfaceTextLength - splitIndex : splitIndexes[index] - splitIndex;
            tokenText = surfaceText.Substring(splitIndex, length);
            splitInstructions.Instructions.Add(new SplitInstruction(splitIndex, tokenText, trainingTexts[index]));
            index++;
        }

        return splitInstructions;
    }

    public static class SplitInstructionErrorMessages
    {
        public static string SplitIndexesMustBeInAscendingOrder = "The split indexes must be in ascending order.";
        public static string SplitIndexesMustBeOneLessThanTrainingTexts = "The number of split indexes must be one less than the number of training texts.";
        public static string TokenTextLengthMustEqualLength = "The 'Length' of each split instruction must equal to the actual length of the instruction's 'TokenText'.";
        public static string AggregatedTokenTextMustEqualSurfaceText = "The aggregated 'TokenText' properties from the 'Instructions' list must be equal to the 'SurfaceText' property of the 'SplitInstructions'.";
    }
    public bool Validate(bool throwIfNotValid = true)
    {
        var errorMessage = new StringBuilder();
        // Validate that the split indexes are in ascending order.
        var indexesValid = Instructions.ValidateIndexesInAscendingOrder(out var message);
        if (!indexesValid && throwIfNotValid)
        {
            var msg = "The split indexes must be in ascending order.";
            throw new SplitInstructionException(SplitInstructionErrorMessages.SplitIndexesMustBeInAscendingOrder, message);
        }

        if (!indexesValid && !throwIfNotValid)
        {
            errorMessage.AppendLine(SplitInstructionErrorMessages.SplitIndexesMustBeInAscendingOrder);
            errorMessage.AppendLine(message);
        }
    

        // Validate that the 'Length' of each split instruction equals the actual length of the 'TokenText'.
        //valid = Instructions.All(i => i.Length == i.TokenText.Length);

        var tokenLengthValid = Instructions.ValidateTokenLengths(out var message2);

        if (!tokenLengthValid && throwIfNotValid)
        {
            throw new SplitInstructionException(SplitInstructionErrorMessages.TokenTextLengthMustEqualLength, message2);
        }

        if (!tokenLengthValid && !throwIfNotValid)
        {
            errorMessage.AppendLine(SplitInstructionErrorMessages.TokenTextLengthMustEqualLength);
            errorMessage.AppendLine(message2);
        }

        // Validate the aggregated TokenText properties from the Instructions list are equal to the SurfaceText property.
            var surfaceTextValid = Instructions.ValidateSurfaceText(SurfaceText, out var message3);
        if (!surfaceTextValid && throwIfNotValid)
        {
            throw new SplitInstructionException(SplitInstructionErrorMessages.AggregatedTokenTextMustEqualSurfaceText, message3);
        }

        if (!surfaceTextValid && !throwIfNotValid)
        {
            errorMessage.AppendLine(SplitInstructionErrorMessages.AggregatedTokenTextMustEqualSurfaceText);
            errorMessage.AppendLine(message3);
        }

        ErrorMessage = errorMessage.ToString();

        return indexesValid && tokenLengthValid && surfaceTextValid;
    }
}