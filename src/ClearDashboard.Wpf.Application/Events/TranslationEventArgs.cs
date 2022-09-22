using System.Windows;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TranslationEventArgs : RoutedEventArgs
    {
        public TokenDisplayViewModel TokenDisplayViewModel { get; set; }

        public Translation Translation { get; set; }
        public string TranslationActionType { get; set; }
    }
}
