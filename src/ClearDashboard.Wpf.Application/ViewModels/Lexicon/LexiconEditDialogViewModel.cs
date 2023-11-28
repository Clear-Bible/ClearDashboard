using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconEditDialogViewModel : DashboardApplicationScreen
    {
      
        private readonly LexiconManager? _lexiconManager;

        private readonly DebounceDispatcher _debounceTimer = new();

        private LexiconManager LexiconManager
        {
            get
            {
                if (_lexiconManager == null) throw new Exception("Cannot perform operation as LexiconManager is null");
                return _lexiconManager!;
            }
            init => Set(ref _lexiconManager, value);
        }

        public LexiconEditDialogState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public LexiconEditMode EditMode
        {
            get => _editMode;
            set => Set(ref _editMode, value);
        }

        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            private set => Set(ref _canSave, value);
        }

        private bool _canCancel = true;
        public bool CanCancel
        {
            get => _canCancel;
            private set => Set(ref _canCancel, value);
        }
        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        public async Task Save()
        {
            var editedLexemes = EditableLexemes.Where(l=>l.Lexeme.Meanings.Any(m=>m.IsDirty) || l.Lexeme.Forms.Any(f=>f.IsDirty)).Select(l=>l.Lexeme).ToList();

            if (editedLexemes.Count == 0) return;

            var lexicon = new DAL.Alignment.Lexicon.Lexicon
            {
                Lexemes = new ObservableCollection<Lexeme>(editedLexemes)
            };

            await lexicon.SaveAsync(Mediator);

            await TryCloseAsync(true);
        }

        public string? SourceLanguage
        {
            get => _sourceLanguage;
            set => Set(ref _sourceLanguage, value);
        }

        public string? SelectedTargetLanguage
        {
            get => _selectedTargetLanguage;
            set => Set(ref _selectedTargetLanguage, value);
        }

        public string? SelectedSourceLanguage
        {
            get => _selectedSourceLanguage;
            set => Set(ref _selectedSourceLanguage, value);
        }

        public string? TargetLanguage
        {
            get => _targetLanguage;
            set => Set(ref _targetLanguage, value);
        }

        public string? ToMatch
        {
            get => _toMatch;
            set => Set(ref _toMatch, value);
        }

        public string? Other
        {
            get => _other;
            set => Set(ref _other, value);
        }

        public string DialogTitle => LocalizationService!.Get("LexiconEdit_Title");
        private LexiconEditMode _editMode;
        private string? _sourceLanguage;
        private string? _targetLanguage;
        private string? _selectedSourceLanguage;
        private string? _selectedTargetLanguage;
        private string? _toMatch;
        private string? _other;
        private BindableCollection<string> _sourceLanguages;
        private BindableCollection<string> _targetLanguages;
        private LexiconEditDialogState _state;

        public BindableCollection<string> SourceLanguages
        {
            get => _sourceLanguages;
            set => Set(ref _sourceLanguages, value);
        }

        public BindableCollection<string> TargetLanguages
        {
            get => _targetLanguages;
            set => Set(ref _targetLanguages, value);
        }

        public bool EnableSourceLanguageComboBox => SelectedSourceLanguage != null && !SelectedSourceLanguage.Equals(SourceLanguage);

        public bool EnableTargetLanguageComboBox => SelectedTargetLanguage != null && !SelectedTargetLanguage.Equals(TargetLanguage);

        public dynamic DialogSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.SingleBorderWindow;
            settings.ShowInTaskbar = false;
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.CanResizeWithGrip;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Top = App.Current.MainWindow.ActualHeight / 2 -200;
            settings.Left = App.Current.MainWindow.ActualWidth / 2 - 258;
            settings.Width = 1000;
            settings.Height = 800;
            settings.Title = DialogTitle;
            return settings;
        }

        public LexiconEditDialogViewModel()
        {
            // required for design-time rendering
        }

        public LexiconEditDialogViewModel(
            LexiconManager lexiconManager,
            DashboardProjectManager? projectManager,
            INavigationService navigationService,
            ILogger<LexiconEditDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,
                localizationService)
        {
            LexiconManager = lexiconManager;
            CanCancel = true;

            TargetLanguages = new BindableCollection<string>();
            SourceLanguages = new BindableCollection<string>();

            State = new LexiconEditDialogState();

           
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            IsBusy = false;
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            IsBusy = true;
            try
            {
                if (!string.IsNullOrEmpty(SourceLanguage))
                {
                    SelectedSourceLanguage = SourceLanguage;
                    SourceLanguages.Add(SourceLanguage);
                }

                if (!string.IsNullOrEmpty(TargetLanguage))
                {
                    SelectedTargetLanguage = TargetLanguage;
                    TargetLanguages.Add(TargetLanguage);
                }


                State.Configure(EditMode, ToMatch);

                EditableLexemes = GetFilteredLexemes();

                EditableLexemes!.HookItemPropertyChanged(UpdateCanSave);
            }
            finally
            {
                IsBusy = false;
            }
            return base.OnActivateAsync(cancellationToken);
        }

        private void UpdateCanSave(object? sender, PropertyChangedEventArgs e)
        {
            var viewModel = (EditableLexemeViewModel)sender!;
            switch (e.PropertyName)
            {
                case nameof(EditableLexemeViewModel.Meanings):
                    CanSave = viewModel.Lexeme.Meanings.Any(m => m.IsDirty);
                    break;
                case nameof(EditableLexemeViewModel.Forms):
                    CanSave = viewModel.Lexeme.Forms.Any(f => f.IsDirty);
                    break;
            }
           
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
          
           
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private BindableCollection<EditableLexemeViewModel>? _editableLexemes;
        public BindableCollection<EditableLexemeViewModel>? EditableLexemes
        {
            get => _editableLexemes;
            set => Set(ref _editableLexemes, value);
        }

        public string EditButtonLabel => GetEditButtonLabel();


        public async Task OnAddButtonClicked(EditableLexemeViewModel editableLexeme)
        {
            switch (EditMode)
            {
                case LexiconEditMode.MatchOnTranslation:
                    editableLexeme.AddNewMeaning(Other);
                    await Task.CompletedTask;
                    break;

                case LexiconEditMode.PartialMatchOnLexemeOrForm:
                   editableLexeme.AddNewForm(Other);
                    await Task.CompletedTask;
                    break;

            }
        }

        public async Task OnEditButtonClicked(EditableLexemeViewModel editableLexeme)
        {
            editableLexeme.IsEditing = !editableLexeme.IsEditing;
            await Task.CompletedTask;
        }

        private string GetEditButtonLabel()
        {
            switch (EditMode)
            {
                case LexiconEditMode.MatchOnTranslation:
                    return string.Format(LocalizationService.Get("LexiconEdit_Edit_PartialMatchOnLemmaOrFormTemplate"), Other);

                case LexiconEditMode.PartialMatchOnLexemeOrForm:
                    return string.Format(LocalizationService.Get("LexiconEdit_Edit_MatchOnTranslationTemplate"), Other);
                   
            }

            return "[UNKNOWN DIALOG MODE!]";
        }

        private BindableCollection<EditableLexemeViewModel>? GetFilteredLexemes()
        {
            var filteredLexemes = new List<Lexeme>();
            var managedLexemes = LexiconManager.ManagedLexemes;

            switch (EditMode)
            {
                case LexiconEditMode.MatchOnTranslation:
                   

                    filteredLexemes = managedLexemes.FilterByTranslationText(SelectedSourceLanguage,
                        SelectedTargetLanguage, Other).ToList();

                    return new BindableCollection<EditableLexemeViewModel>(filteredLexemes.Select(l=> new EditableLexemeViewModel(l) {SourceLanguage = SourceLanguage, TargetLanguage = TargetLanguage}));

                case LexiconEditMode.PartialMatchOnLexemeOrForm:

                    //filteredLexemes = managedLexemes.FilterByLexemeText(ToMatch, State.FormsOption == MatchOption.Partially, SourceLanguage, null).ToList();

                    filteredLexemes = managedLexemes.FilterByLexemeText(Other, State.FormsOption == MatchOption.Partially, SourceLanguage, null).ToList();
                    
                    return new BindableCollection<EditableLexemeViewModel>(filteredLexemes.Select(l => new EditableLexemeViewModel(l) { SourceLanguage = SourceLanguage, TargetLanguage = TargetLanguage }));

                default:
                    return null;

            }
          
        }

        public void OnLexemeOptionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {

                State.LexemeOption =
                    (MatchOption)Enum.Parse(typeof(MatchOption), (item.Tag as string));
                ////_debounceTimer.DebounceAsync(10, async () => await UpdateAlignmentStatuses(approvalType));

            }
        }

        public void OnFormsOptionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {

                State.FormsOption =
                    (MatchOption)Enum.Parse(typeof(MatchOption), (item.Tag as string));
                ////_debounceTimer.DebounceAsync(10, async () => await UpdateAlignmentStatuses(approvalType));

            }
        }

        public void OnPredicateOptionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {

                State.PredicateOption =
                    (PredicateOption)Enum.Parse(typeof(PredicateOption), (item.Tag as string));
                ////_debounceTimer.DebounceAsync(10, async () => await UpdateAlignmentStatuses(approvalType));

            }
        }

        public void OnFormsMatchChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            State.FormsMatch = textBox.Text;
        }

        public void OnTranslationMatchChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            State.TranslationMatch = textBox.Text;
        }

        public void OnTranslationChecked(CheckBox? checkBox)
        {
            //State.TranslationChecked = checkBox?.IsChecked;
        }

        public void OnFormsChecked(CheckBox? checkBox)
        {
            //State.FormsChecked = checkBox?.IsChecked;
        }

        public void OnLexemeChecked(CheckBox? checkBox)
        {
            //State.LexemeChecked = checkBox?.IsChecked;
        }
    }
}
