using Autofac.Core.Lifetime;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
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

        public void TranslationDoubleClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Task.Run(() => TranslationSetAsync(routedEventArgs as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        public void TranslationSet(object sender, RoutedEventArgs routedEventArgs)
        {
            Task.Run(() => TranslationSetAsync(routedEventArgs as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        public async Task TranslationSetAsync(TranslationEventArgs args)
        {
            if (!args.IsControlPressed)
            {
                async Task ShowTranslationSelectionDialog()
                {
                    if (args.InterlinearDisplay != null)
                    {
                        var dialogViewModel = args.InterlinearDisplay.Resolve<LexiconDialogViewModel>();
                        dialogViewModel.TokenDisplay = args.TokenDisplay;
                        dialogViewModel.InterlinearDisplay = args.InterlinearDisplay;
                        _ = await args.InterlinearDisplay.WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
                    }
                }
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowTranslationSelectionDialog);
            }
        }
    }
}
