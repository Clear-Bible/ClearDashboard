using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TokenCharacterEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="TokenCharacterViewModel"/> that triggered this event.
        /// </summary>
        public TokenCharacterViewModel TokenCharacter { get; set; } = new();
    }
}
