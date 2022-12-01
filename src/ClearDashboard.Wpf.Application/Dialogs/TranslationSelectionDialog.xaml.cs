using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Events;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Dialogs
{
    /// <summary>
    /// Interaction logic for TranslationSelectionDialog.xaml
    /// </summary>
    public partial class TranslationSelectionDialog : INotifyPropertyChanged
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

        public string DialogTitle => $"Select Translation: {TokenDisplay?.Token}";

        public TokenDisplayViewModel TokenDisplay { get; }

        public VerseDisplayViewModel VerseDisplay { get; }

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
            if (TokenDisplay.Translation != null && !TokenDisplay.Translation.IsDefault)
            {
                TranslationOptions = await VerseDisplay.GetTranslationOptionsAsync(TokenDisplay.Token);
                CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == TokenDisplay.TargetTranslationText);

                OnUIThread(() =>
                {
                    if (CurrentTranslationOption == null)
                    {
                        TranslationSelectorControl.TranslationValue.Text = TokenDisplay.TargetTranslationText;
                        TranslationSelectorControl.TranslationValue.SelectAll();
                    }
                    TranslationSelectorControl.TranslationOptionsVisibility = Visibility.Visible;
                });
            }
            OnUIThread(() =>
            {
                TranslationSelectorControl.TranslationControlsVisibility = Visibility.Visible;
                ProgressBarVisibility = Visibility.Collapsed;
                TranslationSelectorControl.TranslationValue.Focus();
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }

        private static void OnUIThread(System.Action action) => action.OnUIThread();

        private void OnTranslationApplied(object sender, RoutedEventArgs e)
        {
            Task.Run(() => OnTranslationAppliedAsync(e as TranslationEventArgs ?? throw new InvalidOperationException()).GetAwaiter());
        }

        private async Task OnTranslationAppliedAsync(TranslationEventArgs e)
        {
            try
            {
                if (!TokenDisplay.TargetTranslationText.Equals(e.Translation.TargetTranslationText))
                {
                    OnUIThread(() => ProgressBarVisibility = Visibility.Visible);

                    async Task ApplyTranslation()
                    {
                        await VerseDisplay.PutTranslationAsync(e.Translation, e.TranslationActionType);
                    }

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(ApplyTranslation);
                }

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

        private void OnTranslationCancelled(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => DialogResult = false);
        }

        public TranslationSelectionDialog(TokenDisplayViewModel tokenDisplay, VerseDisplayViewModel verseDisplay)
        {
            InitializeComponent();
            TokenDisplay = tokenDisplay;
            VerseDisplay = verseDisplay;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContext = this;
        }
    }
}
