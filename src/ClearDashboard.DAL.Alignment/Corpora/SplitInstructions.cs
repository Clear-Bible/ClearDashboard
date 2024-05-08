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
            throw new SplitInstructionException("The number of split indexes must be one less than the number of training texts.", $"'splitIndexes' count: {splitIndexes.Count}, 'trainingTexts' count: {trainingTexts.Count}");
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
    public bool Validate(bool throwIfNotValid = true)
    {

        // Validate that the split indexes are in ascending order.
        var valid = Instructions.ValidateIndexesInAscendingOrder(out var message);
        if (!valid && throwIfNotValid)
        {
            throw new SplitInstructionException($"The split indexes must be in ascending order.", message);
        }

        // Validate that the 'Length' of each split instruction equals the actual length of the 'TokenText'.
        valid = Instructions.All(i => i.Length == i.TokenText.Length);

        valid = Instructions.ValidateTokenLengths(out var message2);

        if (!valid && throwIfNotValid)
        {
            throw new SplitInstructionException($"The 'Length' of each split instruction must equal to the actual length of the instruction's 'TokenText'.", message2);
        }

        valid = Instructions.ValidateSurfaceText(SurfaceText, out var message3);
        if (!valid && throwIfNotValid)
        {
            throw new SplitInstructionException("The aggregated 'TokenText' properties from the 'Instructions' list must be equal to the 'SurfaceText' property of the 'SplitInstructions'.", message3);
        }
        return valid;
    }
}