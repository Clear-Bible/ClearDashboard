using ClearBible.Engine.Corpora;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

public record HighlightTokensMessage(bool IsSource, TokenId TokenId);