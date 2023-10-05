using ClearBible.Engine.Corpora;
using ClearDashboard.Wpf.Application.Collections;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record TokenSplitMessage(IDictionary<TokenId, IEnumerable<CompositeToken>> SplitCompositeTokensByIncomingTokenId, 
                                    IDictionary<TokenId, IEnumerable<Token>> SplitChildTokensByIncomingTokenId);
}
