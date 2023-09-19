using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.EnhancedView
{
    /// <summary>
    /// Interaction logic for InterlinearEnhancedViewItemView.xaml
    /// </summary>
    public partial class InterlinearEnhancedViewItemView : UserControl
    {
        public InterlinearEnhancedViewItemView()
        {
            InitializeComponent();
        }

        //public void TranslationClicked(object sender, RoutedEventArgs routedEventArgs)
        //{
        //    Task.Run(() => TranslationClickedAsync(routedEventArgs as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        //}

        //public async Task TranslationClickedAsync(TranslationEventArgs args)
        //{
        //    async Task ShowTranslationSelectionDialog()
        //    {
        //        if (args.InterlinearDisplay != null)
        //        {
        //            var dialogViewModel = args.InterlinearDisplay.Resolve<LexiconDialogViewModel>();
        //            dialogViewModel.TokenDisplay = args.TokenDisplay;
        //            dialogViewModel.InterlinearDisplay = args.InterlinearDisplay;
        //            _ = await args.InterlinearDisplay.WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
        //        }
        //    }
        //    await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowTranslationSelectionDialog);
        //}

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
            if (args.SelectedTokens.Count(t => t.IsTranslationSelected) == 1 &&
                !args.SelectedTokens.Any(t => t.IsTokenSelected))
            {
                async Task ShowTranslationSelectionDialog()
                {
                    if (args.InterlinearDisplay != null)
                    {
                        var dialogViewModel = args.InterlinearDisplay.Resolve<LexiconDialogViewModel>();
                        dialogViewModel.TokenDisplay = args.TokenDisplay!;
                        dialogViewModel.InterlinearDisplay = args.InterlinearDisplay;
                        _ = await args.InterlinearDisplay.WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
                    }
                }
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowTranslationSelectionDialog);
            }
        }

    }
}
