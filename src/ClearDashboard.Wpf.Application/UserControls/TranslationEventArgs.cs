using System.Windows;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public class TranslationEventArgs : RoutedEventArgs
    {
        public Translation Translation { get; set; }
    }
}
