using System.Collections.Generic;
using System.Linq;
using Autofac;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public static class SplitInstructionViewModelFactory
{
    public static SplitInstructionsViewModel CreateSplits(string surfaceText, List<int> splitIndexes, ILifetimeScope? lifetimeScope)
    {

        if (!splitIndexes.ValidateIndexesInAscendingOrder(out string? message))
        {
            throw new SplitInstructionException(SplitInstructions.SplitInstructionErrorMessages.SplitIndexesMustBeInAscendingOrder, message);
        }

        if (!splitIndexes.ValidateFirstAndLastIndexes(surfaceText.Length, out string? message2))
        {
            throw new SplitInstructionException(string.Format(SplitInstructions.SplitInstructionErrorMessages.SplitIndexesMustBeWithinRange, surfaceText.Length), message2);
        }

        var splitInstructions = new SplitInstructionsViewModel
        {
            SurfaceText = surfaceText,
            SplitIndexes = splitIndexes
        };

        var index = 0;
        var surfaceTextLength = surfaceText.Length;
        foreach (var splitIndex in splitIndexes)
        {
            var last = splitIndexes.Last() == splitIndex;
            var first = splitIndexes.First() == splitIndex;

            string tokenText;

            var lexiconDialogViewModel = lifetimeScope?.Resolve<LexiconDialogViewModel>();

            if (first)
            {
                tokenText = surfaceText[..splitIndex];
                splitInstructions.Add(new SplitInstructionViewModel{ Index = 0, TokenText = tokenText, TrainingText = tokenText, TokenType = TokenTypes.Prefix, LexiconDialogViewModel = lexiconDialogViewModel});
                index++;
            }
            
            var length = last ? surfaceTextLength - splitIndex : splitIndexes[index] - splitIndex;
            tokenText = surfaceText.Substring(splitIndex, length);
            splitInstructions.Add(new SplitInstructionViewModel {Index = splitIndex, TokenText = tokenText, TrainingText = tokenText, TokenType = DetermineTokenType(index, splitIndex, surfaceTextLength, splitIndexes.Count), CircumfixGroup = "A"});
            index++;
        }

        return splitInstructions;
    }

    private static string DetermineTokenType(int index, int splitIndex, int surfaceTextLength, int splitIndexCount)
    {
        if (index == 0)
        {
            return TokenTypes.Prefix;
        }

        if (splitIndexCount == 1)
        {
            return TokenTypes.Stem;
        }

        if (splitIndexCount > 1)
        {
            return splitIndex < splitIndexCount ? TokenTypes.Stem : TokenTypes.Suffix;
        }

        return TokenTypes.Stem;

        //return splitIndex < splitIndexCount-1 ? TokenTypes.Stem : TokenTypes.Suffix;

        //if (index == surfaceTextLength || index == splitIndex && splitIndex > 1)
        //{
        //    return TokenTypes.Suffix;
        //}

        //return TokenTypes.Stem;
    }

}