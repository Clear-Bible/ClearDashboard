using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconViewModel : ToolViewModel
    {
        #region Member Variables

        private IWindowManager WindowManager { get; }
        private LexiconManager LexiconManager { get; }

        #endregion //Member Variables


        #region Public Properties

        public BindableCollection<LexiconImportViewModel> LexiconToImport { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        public bool HasLexiconToImport => LexiconToImport.Any();
       
        public BindableCollection<LexiconImportViewModel> ImportedLexicon { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        public List<CorpusId> ProjectCorpora { get; } = new List<CorpusId>();

        public CorpusId? SelectedProjectCorpus
        {
            get => _selectedProjectCorpus;
            set
            {
                if (Set(ref _selectedProjectCorpus, value))
                {
                    NotifyOfPropertyChange<bool>(()=>HasSelectedProjectCorpus);
                    NotifyOfPropertyChange<bool>(()=>ShowNoRecordsToManageMessage);
                }
            }
        }

        public bool HasSelectedProjectCorpus => SelectedProjectCorpus != null;

        public bool ShowNoRecordsToManageMessage => HasSelectedProjectCorpus && !LexiconToImport.Any();

        public bool ShowDialog
        {
            get => _showDialog;
            set => Set(ref _showDialog, value);
        }

        #endregion //Public Properties


        #region Observable Properties

        private Visibility _progressBarVisibility = Visibility.Hidden;
        private bool _showDialog;

        private CorpusId? _selectedProjectCorpus;

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public LexiconViewModel()
        {
            //required for design-time support
        }

        public LexiconViewModel(INavigationService navigationService,
            ILogger<LexiconViewModel> logger,
            DashboardProjectManager dashboardProjectManager,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope lifetimeScope,
            ILocalizationService localizationService,
            IWindowManager windowManager, 
            LexiconManager lexiconManager) :
            base(navigationService, logger, dashboardProjectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            LexiconManager = lexiconManager;
            WindowManager = windowManager;
        }


        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                // TODO:  change to true when feature is ready.
                ShowDialog = false;
                ProgressBarVisibility = Visibility.Visible;
                var stopWatch = Stopwatch.StartNew();
                try
                {
                    await GetParatextProjects(); 
                    await GetImportedLexiconViewModels(cancellationToken);
                }
                finally
                {
                    stopWatch.Stop();
                    Logger!.LogInformation($"Loaded lexicon data in {stopWatch.ElapsedMilliseconds} milliseconds.");

                    Execute.OnUIThread(() =>
                    {
                        ProgressBarVisibility = Visibility.Hidden;
                    });
                }
            }, cancellationToken);
            await base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }

        #endregion //Constructor


        #region Methods

        private async Task GetParatextProjects()
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            foreach (var corpusId in topLevelProjectIds.CorpusIds.Where(c=> c.CorpusType == CorpusType.Standard.ToString() || c.CorpusType == CorpusType.BackTranslation.ToString() || c.CorpusType == CorpusType.Resource.ToString()).OrderBy(c => c.Created))
            {
                ProjectCorpora.Add(corpusId);
            }

            await Task.CompletedTask;
        }
        private async Task GetImportedLexiconViewModels(CancellationToken cancellationToken)
        {
            ImportedLexicon.Clear();
            var importedLexicon = await LexiconManager.GetImportedLexiconViewModels(null, cancellationToken);
            Execute.OnUIThread(() => { ImportedLexicon.AddRange(importedLexicon); });
        }

        private async Task GetToImportLexiconImportViewModels(CancellationToken cancellationToken)
        {
            try
            {
                ProgressBarVisibility = Visibility.Visible;
                LexiconToImport.Clear();
                NotifyOfPropertyChange(()=>HasLexiconToImport);
                NotifyOfPropertyChange(()=>ShowNoRecordsToManageMessage);

                var projectId = SelectedProjectCorpus?.ParatextGuid;
                var lexiconImports = await LexiconManager.GetLexiconImportViewModels(projectId, cancellationToken);
                Execute.OnUIThread(() => { LexiconToImport.AddRange(lexiconImports); });
                NotifyOfPropertyChange(() => HasLexiconToImport);
                NotifyOfPropertyChange(() => ShowNoRecordsToManageMessage);
            }
            finally
            {
                Execute.OnUIThread(() =>
                {
                    ProgressBarVisibility = Visibility.Hidden;
                });
            }
           
        }

        public async Task ProjectCorpusSelected()
        {
            await GetToImportLexiconImportViewModels(CancellationToken.None);
        }

        public async Task OnAddAsFormButtonClicked(LexiconImportViewModel lexiconImport)
        {
            if (!ShowDialog)
            {
                return;
            }

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("lexiconManager", LexiconManager),
               
            };

            var dialogViewModel = LifetimeScope?.Resolve<LexiconEditDialogViewModel>(parameters);
            if (dialogViewModel != null)
            {
                dialogViewModel.SourceLanguage = lexiconImport.SourceLanguage;
                dialogViewModel.TargetLanguage = lexiconImport.TargetLanguage;
                dialogViewModel.EditMode = LexiconEditMode.PartialMatchOnLexemeOrForm;
                dialogViewModel.ToMatch = lexiconImport.TargetWord;
                dialogViewModel.Other = lexiconImport.SourceWord;

                await dialogViewModel.ActivateAsync();

                var result = await WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
            }
        }


        public async Task OnTargetAsTranslationButtonClicked(LexiconImportViewModel lexiconImport)
        {

            if (!ShowDialog)
            {
                return;
            }
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("lexiconManager", LexiconManager),

            };
            var dialogViewModel = LifetimeScope?.Resolve<LexiconEditDialogViewModel>(parameters);
            if (dialogViewModel != null)
            {
                dialogViewModel.SourceLanguage = lexiconImport.SourceLanguage;
                dialogViewModel.TargetLanguage = lexiconImport.TargetLanguage;
                dialogViewModel.EditMode = LexiconEditMode.MatchOnTranslation;
                dialogViewModel.ToMatch = lexiconImport.SourceWord;
                dialogViewModel.Other = lexiconImport.TargetWord;

                await dialogViewModel.ActivateAsync();

                var result = await WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }


        public void OnToggleAllChecked(CheckBox? checkBox)
        {
            if (checkBox != null && LexiconToImport is { Count: > 0 })
            {
                foreach (var lexicon in LexiconToImport)
                {
                    lexicon.IsSelected = checkBox.IsChecked ?? false;
                }
            }
        }

        public async Task ProcessLexiconToImport()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Execute.OnUIThread(() => { ProgressBarVisibility = Visibility.Visible; });

                    await LexiconManager.ProcessLexiconToImport();

                    await GetImportedLexiconViewModels(CancellationToken.None);
                    await GetToImportLexiconImportViewModels(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An unexpected error occurred while importing the Lexicon.");
                }
                finally
                {
                    Execute.OnUIThread(() => { ProgressBarVisibility = Visibility.Hidden; });
                }
            });

        }

        #endregion // Methods
    }
}
