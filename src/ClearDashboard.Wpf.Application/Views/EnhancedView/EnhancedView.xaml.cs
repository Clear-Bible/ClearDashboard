using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.Events;

namespace ClearDashboard.Wpf.Application.Views.EnhancedView
{
    /// <summary>
    /// Interaction logic for EnhancedView.xaml
    /// </summary>
    public partial class EnhancedView : UserControl
    {
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

        public EnhancedView()
        {
            InitializeComponent();
        }
    }
}
