using System.ComponentModel;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A lightweight control for displaying a verse with no interactivity.
    /// </summary>
    public partial class ReadOnlyVerseDisplay
    {
        public ReadOnlyVerseDisplay()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(SourceFlowDirection));
            OnPropertyChanged(nameof(TargetFlowDirection));
            OnPropertyChanged(nameof(SourceTokens));
            OnPropertyChanged(nameof(TargetTokens));
            OnPropertyChanged(nameof(TargetVisibility));
            Loaded -= OnLoaded;
        }
    }
}
