using ClearBible.Engine.Corpora;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record TokenUnjoinedMessage(CompositeToken CompositeToken, TokenCollection Tokens);
}
