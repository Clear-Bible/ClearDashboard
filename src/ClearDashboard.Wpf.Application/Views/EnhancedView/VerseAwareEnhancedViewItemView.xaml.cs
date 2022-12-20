using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.Events;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.EnhancedView
{
    /// <summary>
    /// Interaction logic for VerseAwareEnhancedViewItemView.xaml
    /// </summary>
    public partial class VerseAwareEnhancedViewItemView : UserControl
    {
        public VerseAwareEnhancedViewItemView()
        {
            InitializeComponent();
        }


        public void TranslationClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Task.Run(() => TranslationClickedAsync(routedEventArgs as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        public async Task TranslationClickedAsync(TranslationEventArgs args)
        {
            void ShowTranslationSelectionDialog()
            {
                var dialog = new TranslationSelectionDialog(args.TokenDisplay!, args.InterlinearDisplay!)
                {
                    Owner = Window.GetWindow(this),
                };
                dialog.ShowDialog();
            }
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowTranslationSelectionDialog);
        }
    }
}
