using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Threading;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{

    public enum LexiconEditMode
    {
        PartialMatchOnLexemeOrForm,
        MatchOnTranslation,
        Edit
    }

    public static class LexiconEditTags
    {
        public const string And = "And";
        public const string Fully = "Fully";
        public const string Or = "Or";
        public const string Partially = "Partially";
       
    }

    public enum PredicateOption
    {
        And,
        Or
    }

    public enum MatchOption
    {
        Partially,
        Fully,
    }

    public class LexiconEditDialogState : PropertyChangedBase
    {
        private bool _lexemeChecked;
        private bool _formsChecked;
        private bool _translationChecked;
        private MatchOption _lexemeOption;
        private MatchOption _formsOption;
        private PredicateOption _predicateOption;
        private string? _formsMatch;
        private string? _translationMatch;

        public bool LexemeChecked
        {
            get => _lexemeChecked;
            set
            {
                Set(ref _lexemeChecked, value);
                NotifyBooleansChanged();
            }
        }

        public bool FormsChecked
        {
            get => _formsChecked;
            set
            {
                Set(ref _formsChecked, value);
                NotifyBooleansChanged();
            }
        }

        public bool LexemeAndFormsChecked => LexemeChecked & FormsChecked;

        public bool LexemeOrFormsChecked => LexemeChecked | FormsChecked;

        public bool TransitionAndLexemeOrFormsChecked => TranslationChecked & LexemeAndFormsChecked;

        private void NotifyBooleansChanged()
        {
            NotifyOfPropertyChange(()=> LexemeAndFormsChecked);
            NotifyOfPropertyChange(() => LexemeOrFormsChecked);
            NotifyOfPropertyChange(() => TransitionAndLexemeOrFormsChecked);
        }

        public bool TranslationChecked
        {
            get => _translationChecked;
            set
            {
                Set(ref _translationChecked, value);
                NotifyBooleansChanged();
            }
        }

        public MatchOption LexemeOption
        {
            get => _lexemeOption;
            set => Set(ref _lexemeOption, value);
        }

        public MatchOption FormsOption
        {
            get => _formsOption;
            set => Set(ref _formsOption, value);
        }

        public PredicateOption PredicateOption
        {
            get => _predicateOption;
            set => Set(ref _predicateOption, value);
        }

        public string? FormsMatch
        {
            get => _formsMatch;
            set => Set(ref _formsMatch, value);
        }

        public string? TranslationMatch
        {
            get => _translationMatch;
            set => Set(ref _translationMatch, value);
        }

        public void Configure(LexiconEditMode editMode, string? toMatch)
        {
            /*
             
              -when [PartialMatchOnLexemeOrForm] parameter: (C) checked, (D) set to
              partially, (F) checked, (G) partial, (I) filled in with [toMatch] parameter,
              (K) unchecked; (M) is "Edit adding [other] as translation to first meaning" and
              when pressed (M) changes line into in-place editing, adds [] to first default meaning 
              if no meaning, then adds [other] to first meaning's comma delimited [] list and selects 
              other in this list so user can see what has been added.
               
            
             -when [MatchOnTranslation] parameter: (C) and (F) unchecked, (K) checked; (L) set to 
               [toMatch].; (M) is "Edit adding [other] as form" and when pressed (M) changes line 
               into in-place editing, adds [other] to the comma delimited list of forms, and highlights 
               added form so user can see what was added..

            */

            switch (editMode)
            {
                case LexiconEditMode.MatchOnTranslation:
                    LexemeChecked = false;
                    FormsChecked = false;
                    TranslationChecked = true;
                    LexemeOption = MatchOption.Partially;
                    FormsOption = MatchOption.Partially;
                    PredicateOption = PredicateOption.And;
                    FormsMatch = string.Empty;
                    TranslationMatch = toMatch;
                    break;
                case LexiconEditMode.PartialMatchOnLexemeOrForm:
                    LexemeChecked = true;
                    FormsChecked = true;
                    TranslationChecked = false;
                    LexemeOption = MatchOption.Partially;
                    FormsOption = MatchOption.Partially;
                    PredicateOption = PredicateOption.And;
                    FormsMatch = toMatch;
                    TranslationMatch = string.Empty;
                    break;

            }
        }

      
    }
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

        public async void Save()
        {
            // TODO...
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

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
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

            FilteredLexemes = FilterLexemes();
            return base.OnActivateAsync(cancellationToken);
        }

        private ObservableCollection<Lexeme>? _filteredLexemes;

        public ObservableCollection<Lexeme>? FilteredLexemes
        {
            get => _filteredLexemes;
            set => Set(ref _filteredLexemes, value);
        }

        private ObservableCollection<Lexeme>? FilterLexemes()
        {
            var filteredLexemes = new List<Lexeme>();

            switch (EditMode)
            {
                case LexiconEditMode.MatchOnTranslation:
                    filteredLexemes =  LexiconManager.ManagedLexemes
                        .Where(lexeme => lexeme.Language == SelectedSourceLanguage && lexeme.Meanings
                            .Any(meaning => meaning.Language == SelectedSourceLanguage && meaning.Translations
                                .Any(trans => trans.Text == State.TranslationMatch)))
                        .ToList();

                    return new ObservableCollection<Lexeme>(filteredLexemes);

                case LexiconEditMode.PartialMatchOnLexemeOrForm:
                    //filteredLexemes = LexiconManager.ManagedLexemes
                    //    .Where(lexeme => lexeme.Language == SelectedSourceLanguage && lexeme.Meanings
                    //        .Any(meaning => meaning.Language == SelectedSourceLanguage && meaning.Text == State.FormsMatch) || 
                    //            lexeme.Forms.Any(form=>form.Text == State.FormsMatch))
                    //    .ToList();

                    filteredLexemes = LexiconManager.ManagedLexemes
                        .Where(lexeme => lexeme.Language == SelectedSourceLanguage && lexeme.Meanings
                            .Any(meaning => meaning.Language == SelectedSourceLanguage && meaning.Translations
                                .Any(trans => trans.Text == State.FormsMatch)))
                        .ToList();

                    return new ObservableCollection<Lexeme>(filteredLexemes);

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
