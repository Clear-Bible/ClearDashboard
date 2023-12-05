using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
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

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class LexiconImportsViewModel: DashboardApplicationScreen
    {
        #region Member Variables   

        private IWindowManager WindowManager { get; }
        private LexiconManager LexiconManager { get; }

        private Visibility _progressBarVisibility = Visibility.Hidden;
        private bool _showDialog;

        private CorpusId? _selectedProjectCorpus;

        #endregion //Member Variables


        #region Public Properties

        public BindableCollection<LexiconImportViewModel> LexiconToImport { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        public bool HasLexiconToImport => Enumerable.Any<LexiconImportViewModel>(LexiconToImport);

        //public BindableCollection<LexiconImportViewModel> ImportedLexicon { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        public List<CorpusId> ProjectCorpora { get; } = new List<CorpusId>();

        public bool HasSelectedProjectCorpus => SelectedProjectCorpus != null;

        public bool ShowNoRecordsToManageMessage => HasSelectedProjectCorpus && !Enumerable.Any<LexiconImportViewModel>(LexiconToImport);

        //public string? NoRecordsToManageMessage => LocalizationService.Get("NoRecordsToManageMessage");

        public bool ShowDialog
        {
            get => _showDialog;
            set => Set(ref _showDialog, value);
        }

        public Guid SelectedProjectId { get; set; } = Guid.Empty;

        #endregion //Public Properties


        #region Observable Properties

        public CorpusId? SelectedProjectCorpus
        {
            get => _selectedProjectCorpus;
            set
            {
                if (Set(ref _selectedProjectCorpus, value))
                {
                    NotifyOfPropertyChange(() => HasSelectedProjectCorpus);
                    NotifyOfPropertyChange(() => ShowNoRecordsToManageMessage);
                }
            }
        }

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public LexiconImportsViewModel()
        {
            //required for design-time support
        }

        public LexiconImportsViewModel(INavigationService navigationService,
            ILogger<LexiconViewModel> logger,
            DashboardProjectManager dashboardProjectManager,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope lifetimeScope,
            ILocalizationService localizationService,
            IWindowManager windowManager,
            LexiconManager lexiconManager) :
            base(dashboardProjectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            LexiconManager = lexiconManager;
            WindowManager = windowManager;
            Message = LocalizationService.Get("LexiconEdit_LoadingData");
        }


        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            // leave the following as not awaited so that the UI can load while the data is being loaded.
            Task.Run(async () =>
            {
                // TODO:  change to true when feature is ready.
                ShowDialog = true;
                ProgressBarVisibility = Visibility.Visible;
                var stopWatch = Stopwatch.StartNew();
                try
                {
                    await GetParatextProjects();
                    await GetToImportLexiconImportViewModels(cancellationToken);

                    if (LexiconToImport.Count == 0)
                    {
                        Message = LocalizationService.Get("LexiconImport_NoMoreRecordsToImport");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An unexpected error occurred while loading the Lexicon data.");
                    Message = string.Format(LocalizationService.Get("LexiconEdit_LoadingData_ErrorTemplate"), ex.Message);
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

        #endregion //Constructor


        #region Methods

        private async Task GetParatextProjects()
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            // this really only is used for the dropdown so we don't really need this
            ProjectCorpora.Clear();
            foreach (var corpusId in topLevelProjectIds.CorpusIds.Where(c =>
                         c.CorpusType == CorpusType.Standard.ToString() ||
                         c.CorpusType == CorpusType.BackTranslation.ToString()).OrderBy(c => c.Created))
            {
                ProjectCorpora.Add(corpusId);
            }

            if (SelectedProjectId != Guid.Empty)
            {
                var selectedProjectCorpus = ProjectCorpora.FirstOrDefault(c => c.Id == SelectedProjectId);

                if (selectedProjectCorpus is not null)
                {
                    //ProjectCorpora.Clear();
                    //ProjectCorpora.Add(selectedProjectCorpus);
                    SelectedProjectCorpus = selectedProjectCorpus;
                    //await ProjectCorpusSelected();
                }
            }

            await Task.CompletedTask;
        }

        //private async Task GetImportedLexiconViewModels(CancellationToken cancellationToken)
        //{
        //    ImportedLexicon.Clear();
        //    var importedLexicon = await LexiconManager.GetImportedLexiconViewModels(null, cancellationToken);
        //    Execute.OnUIThread(() => { ImportedLexicon.AddRange(importedLexicon); });
        //}

        private async Task GetToImportLexiconImportViewModels(CancellationToken cancellationToken)
        {
            try
            {
                ProgressBarVisibility = Visibility.Visible;
                LexiconToImport.Clear();
                NotifyOfPropertyChange(() => HasLexiconToImport);
                NotifyOfPropertyChange(() => ShowNoRecordsToManageMessage);

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

        //public async Task ProjectCorpusSelected(SelectionChangedEventArgs args)
        //{
        //    await GetToImportLexiconImportViewModels(CancellationToken.None);
        //}

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
                dialogViewModel.EditMode = LexiconEditMode.MatchOnTranslation;
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
                dialogViewModel.EditMode = LexiconEditMode.PartialMatchOnLexemeOrForm;
                dialogViewModel.ToMatch = lexiconImport.SourceWord;
                dialogViewModel.Other = lexiconImport.TargetWord;

                await dialogViewModel.ActivateAsync();


                var dialogSettings = dialogViewModel.DialogSettings();
                //dialogSettings.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                dialogSettings.Top = App.Current.MainWindow.ActualHeight / 2 - 800;
                dialogSettings.Left = App.Current.MainWindow.ActualWidth / 2 - 258;

                var result = await WindowManager.ShowDialogAsync(dialogViewModel, null, dialogSettings());
            }
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


        public async void Close()
        {
            await TryCloseAsync();
        }

        public async Task ProcessLexiconToImport()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Execute.OnUIThread(() => { ProgressBarVisibility = Visibility.Visible; });

                    await LexiconManager.ProcessLexiconToImport();

                    //await GetImportedLexiconViewModels(CancellationToken.None);
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
