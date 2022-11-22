using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.Events;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Views.Display
{
    public partial class EnhancedViewDemoView
    {
        public void TranslationClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Task.Run(() => TranslationClickedAsync(routedEventArgs as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        public async Task TranslationClickedAsync(TranslationEventArgs args)
        {
            void ShowTranslationSelectionDialog()
            {
                var dialog = new TranslationSelectionDialog(args.TokenDisplay!, args.VerseDisplay!)
                {
                    Owner = Window.GetWindow(this),
                };
                dialog.ShowDialog();
            }
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowTranslationSelectionDialog);
        }

        public EnhancedViewDemoView()
        {
            InitializeComponent();
        }
    }
}
