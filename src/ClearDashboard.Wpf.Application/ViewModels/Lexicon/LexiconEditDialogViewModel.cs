using System;
using System.Collections.Generic;
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

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{

    public enum LexiconEditMode
    {
        PartialMatchOnLexemeOrForm,
        MatchOnTranslation,
        Edit
    }
    public class LexiconEditDialogViewModel : DashboardApplicationScreen
    {
        private readonly LexiconManager? _lexiconManager;
        private LexiconManager LexiconManager
        {
            get
            {
                if (_lexiconManager == null) throw new Exception("Cannot perform operation as LexiconManager is null");
                return _lexiconManager!;
            }
            init => Set(ref _lexiconManager, value);
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
            
            return base.OnActivateAsync(cancellationToken);
        }
    }
}
