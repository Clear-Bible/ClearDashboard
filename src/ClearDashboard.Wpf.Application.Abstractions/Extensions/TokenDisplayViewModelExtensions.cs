using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Extensions;

public static class TokenDisplayViewModelExtensions
{

    // see https://stackoverflow.com/questions/14503691/right-to-left-language-bracket-reversed
    private static string PushRtl => "\u200f";
    private static string PopLtr => "\u200e";

    public static string GetDisplayText(this TokenDisplayViewModel token)
    {
        var text = token.AlignmentToken.GetDisplayText();

        // Is this an RTL source token?
        if (token.IsSource && token.IsSourceRtl)
        {
            // Fix the RTL flow direction by temporarily pushing RTL and  
            // then resetting back to LTR during string interpolation
            return $"{PushRtl}{text}{PopLtr}";
        }

        // Is this an RTL target token?
        if (token.IsTarget && token.IsTargetRtl)
        {
            // Fix the RTL flow direction by temporarily pushing RTL and  
            // then resetting back to LTR during string interpolation
            return $"{PushRtl}{text}{PopLtr}";
        }

        // The token (either source or target) is not RTL,
        // so return the the display text as is.
        return text;
    }
}