using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Events;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Dialogs
{
    /// <summary>
    /// Interaction logic for TranslationSelectionDialog.xaml
    /// </summary>
    public partial class TranslationLexiconDialog : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public string DialogTitle => $"Translation/Lexeme: {TokenDisplay?.TranslationSurfaceText}";

        public TokenDisplayViewModel TokenDisplay { get; }

        public InterlinearDisplayViewModel InterlinearDisplay { get; }

        private LexemeViewModel? _lexeme;
        public LexemeViewModel? Lexeme
        {
            get => _lexeme;
            set => SetField(ref _lexeme, value);
        }

        private SemanticDomainCollection _semanticDomainSuggestions = new();
        public SemanticDomainCollection SemanticDomainSuggestions
        {
            get => _semanticDomainSuggestions;
            set => SetField(ref _semanticDomainSuggestions, value);
        }

        private LexiconTranslationViewModelCollection _concordance = new();
        public LexiconTranslationViewModelCollection Concordance
        {
            get => _concordance;
            set => SetField(ref _concordance, value);
        }

        private IEnumerable<TranslationOption> _translationOptions = new List<TranslationOption>();
        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            private set => SetField(ref _translationOptions, value);
        }

        private TranslationOption? _currentTranslationOption;
        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            private set => SetField(ref _currentTranslationOption, value);
        }

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => SetField(ref _progressBarVisibility, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => OnLoadedAsync().GetAwaiter());
        }

        private async Task OnLoadedAsync()
        {
            //if (TokenDisplay.Translation != null && !TokenDisplay.Translation.IsDefault)
            //{
            //    TranslationOptions = await InterlinearDisplay.GetTranslationOptionsAsync(TokenDisplay.Token);
            //    CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == TokenDisplay.TargetTranslationText);

            //    OnUIThread(() =>
            //    {
            //        if (CurrentTranslationOption == null)
            //        {
            //            TranslationSelectorControl.TranslationValue.Text = TokenDisplay.TargetTranslationText;
            //            TranslationSelectorControl.TranslationValue.SelectAll();
            //        }
            //        TranslationSelectorControl.TranslationOptionsVisibility = Visibility.Visible;
            //    });
            //}
            //OnUIThread(() =>
            //{
            //    TranslationSelectorControl.TranslationControlsVisibility = Visibility.Visible;
            //    ProgressBarVisibility = Visibility.Collapsed;
            //    TranslationSelectorControl.TranslationValue.SelectAll();
            //    TranslationSelectorControl.TranslationValue.Focus();
            //});
            Loaded -= OnLoaded;
        }

        private static void OnUIThread(System.Action action) => action.OnUIThread();

        private void ApplyTranslation(object sender, RoutedEventArgs e)
        {
            Task.Run(() => ApplyTranslationAsync(e as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        private async Task ApplyTranslationAsync(TranslationEventArgs e)
        {
            try
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Visible);

                async Task SaveTranslation()
                {
                    await InterlinearDisplay.PutTranslationAsync(e.Translation, e.TranslationActionType);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(SaveTranslation);

                System.Windows.Application.Current.Dispatcher.Invoke(() => DialogResult = true);
            }
            catch (Exception ex)
            {
                var exception = e;
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
            }
        }

        private void CancelTranslation(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => DialogResult = false);
        }

        public TranslationLexiconDialog(TokenDisplayViewModel tokenDisplay, InterlinearDisplayViewModel interlinearDisplay)
        {
            InitializeComponent();
            TokenDisplay = tokenDisplay;
            InterlinearDisplay = interlinearDisplay;

            Loaded += OnLoaded;
            DataContext = this;
        }

    }
}
