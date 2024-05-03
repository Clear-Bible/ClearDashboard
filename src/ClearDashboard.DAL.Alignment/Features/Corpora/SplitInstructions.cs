namespace ClearDashboard.DAL.Alignment.Features.Corpora;


public class SplitIndexException : Exception
{
    public SplitIndexException(string message) : base(message)
    {
    }
}

public static class SplitInstructionsExtensions
{
    public static void LogSplitInstruction(SplitInstruction splitInstruction)
    {
        Console.WriteLine($"Index: {splitInstruction.Index}, Length: {splitInstruction.Length}, TokenText: {splitInstruction.TokenText}, TrainingText: {splitInstruction.TrainingText}");
    }

    public static void ValidateIndexes(this List<int> splitIndexes)
    {
        var valid = splitIndexes.Take(splitIndexes.Count - 1).All(n => n > splitIndexes.Last());
    }
}

public class SplitInstructions : List<SplitInstruction>
{

    public string? TokenText { get; private init; }

    public int? TokenTextLength => TokenText?.Length;

    public static SplitInstructions CreateSplits(string surfaceText, List<int> splitIndexes, List<string?> trainingTexts)
    {
          
        var surfaceTextLength = surfaceText.Length;
        if (splitIndexes.Count  != trainingTexts.Count - 1)
        {
            throw new ArgumentException("The number of split indexes must be one less than the number of training texts");
        }

        var splitInstructions = new SplitInstructions
        {
            TokenText = surfaceText
        };

        var index = 0;
        foreach (var splitIndex in splitIndexes)
        {
            var last = splitIndexes.Last() == splitIndex;
            var first = splitIndexes.First() == splitIndex;
               
            string tokenText;

            if (first)
            {
                tokenText = surfaceText.Substring(0, splitIndex);
                splitInstructions.Add(new SplitInstruction(0, splitIndex, tokenText, trainingTexts[index]));
                index++;
            }

            var length = last ? surfaceTextLength - splitIndex : splitIndexes[index] - splitIndex;
            tokenText = surfaceText.Substring(splitIndex, length);
            splitInstructions.Add(new SplitInstruction(splitIndex, length, tokenText, trainingTexts[index]));
            index++;
        }

        return splitInstructions;
    }
    //public bool Validate()
    //{
    //    var indexSum = this.Sum(i => i.Index);
    //    var lengthSum = this.Sum(i => i.Length);

    //    return (indexSum) == TokenTextLength && lengthSum == TokenTextLength;
    //}
}