using System;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.UserControls;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconViewModel : ToolViewModel
    {
        private IWindowManager WindowManager { get; }
        private LexiconManager LexiconManager { get; }

        private Visibility _progressBarVisibility = Visibility.Hidden;
        private bool _enableButtons;
        private bool _hasLexiconImports;
        private CorpusId? _selectedProjectCorpus;

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }
        public LexiconViewModel()
        {

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

        public BindableCollection<LexiconImportViewModel> LexiconImports { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        public bool HasLexiconImports => LexiconImports.Any();
       
        public BindableCollection<LexiconImportViewModel> ImportedLexicon { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        public List<CorpusId> ProjectCorpora { get; } = new List<CorpusId>();

        public CorpusId? SelectedProjectCorpus
        {
            get => _selectedProjectCorpus;
            set
            {
                if (Set(ref _selectedProjectCorpus, value))
                {
                    NotifyOfPropertyChange(()=>HasSelectedProjectCorpus);
                    NotifyOfPropertyChange(()=>ShowNoRecordsToManageMessage);
                }
            }
        }

        public bool HasSelectedProjectCorpus => SelectedProjectCorpus != null;

        public bool ShowNoRecordsToManageMessage => HasSelectedProjectCorpus && !LexiconImports.Any();

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                // TODO:  change to true when feature is ready.
                EnableButtons = false;
                ProgressBarVisibility = Visibility.Visible;
                var stopWatch = Stopwatch.StartNew();
                try
                {
                    await GetParatextProjects(); 
                    //await GetLexiconImportViewModels(cancellationToken);

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

        private async Task GetParatextProjects()
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            foreach (var corpusId in topLevelProjectIds.CorpusIds.Where(c=> c.CorpusType == CorpusType.Standard.ToString() || c.CorpusType != CorpusType.BackTranslation.ToString()).OrderBy(c => c.Created))
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

        private async Task GetLexiconImportViewModels(CancellationToken cancellationToken)
        {
            try
            {
                ProgressBarVisibility = Visibility.Visible;
                LexiconImports.Clear();
                NotifyOfPropertyChange(()=>HasLexiconImports);
                NotifyOfPropertyChange(()=>ShowNoRecordsToManageMessage);

                var projectId = SelectedProjectCorpus?.ParatextGuid;
                var lexiconImports = await LexiconManager.GetLexiconImportViewModels(projectId, cancellationToken);
                Execute.OnUIThread(() => { LexiconImports.AddRange(lexiconImports); });
                NotifyOfPropertyChange(() => HasLexiconImports);
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

        public async Task ProjectCorpusSelected(SelectionChangedEventArgs args)
        {
            var selectedCorpusId = (CorpusId?)args.AddedItems[0];
            await GetLexiconImportViewModels(CancellationToken.None);
        }

        public async Task ProjectCorpusSelected()
        {
            await GetLexiconImportViewModels(CancellationToken.None);
        }

        public async Task OnAddAsFormButtonClicked(LexiconImportViewModel lexiconImport)
        {
            var dialogViewModel = LifetimeScope?.Resolve<LexiconEditDialogViewModel>();
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


        public bool EnableButtons
        {
            get => _enableButtons;
            set => Set(ref _enableButtons, value);
        }

        public async Task OnTargetAsTranslationButtonClicked(LexiconImportViewModel lexiconImport)
        {
            var dialogViewModel = LifetimeScope?.Resolve<LexiconEditDialogViewModel>();
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


        public void OnToggleAllChecked(CheckBox? checkBox)
        {
            if (checkBox != null && LexiconImports is { Count: > 0 })
            {
                foreach (var lexicon in LexiconImports)
                {
                    lexicon.IsSelected = checkBox.IsChecked ?? false;
                }
            }
        }

        public async Task ProcessImportedLexicon()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Execute.OnUIThread(() => { ProgressBarVisibility = Visibility.Visible; });

                    await LexiconManager.ProcessImportedLexicon();

                    //SelectedProjectCorpus = null;
                    LexiconImports.Clear();
                    await GetImportedLexiconViewModels(CancellationToken.None);
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
    }
}
