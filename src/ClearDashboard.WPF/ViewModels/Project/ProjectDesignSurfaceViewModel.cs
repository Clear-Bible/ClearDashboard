using Caliburn.Micro;
//using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Design;
using System.Windows.Threading;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Interceptors;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using Brushes = System.Windows.Media.Brushes;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using Rectangle = System.Windows.Shapes.Rectangle;
using ClearDashboard.DataAccessLayer.Models.Common;
using Token = ClearBible.Engine.Corpora.Token;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public record CorporaLoadedMessage(IEnumerable<Corpus> Copora);

    public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, ParatextProjectMetadata ProjectMetadata);

    public class ProjectDesignSurfaceViewModel : ToolViewModel, IHandle<BackgroundTaskChangedMessage>
    {
        CancellationTokenSource _tokenSource = null;
        private bool _addParatextCorpusRunning = false;
        public IWindowManager WindowManager { get; }
        private readonly IMediator _mediator;
        private ObservableCollection<Corpus> _corpora;
        public ObservableCollection<Corpus> Corpora
        {
            get => _corpora;
            set => _corpora = value;
        }

        public ProjectDesignSurfaceViewModel()
        {

        }

        public ProjectDesignSurfaceViewModel(IMediator mediator, IWindowManager windowManager, INavigationService navigationService, ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            WindowManager = windowManager;
            _mediator = mediator;
            Title = "🖧 PROJECT DESIGN SURFACE";
            ContentId = "PROJECTDESIGNSURFACETOOL";

            Corpora = new ObservableCollection<Corpus>();
            _tokenSource = new CancellationTokenSource();
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //IsBusy = true;
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            //IsBusy = false;
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            //we need to cancel this process here
            //check a bool to see if it already cancelled or already completed
            if (_addParatextCorpusRunning)
            {
                _tokenSource.Cancel();
                EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = "Corpus",
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskStatus = StatusEnum.Completed
                }));
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public ProjectDesignSurfaceView View { get; set; }
        public Canvas DesignSurfaceCanvas { get; set; }
        protected override async void OnViewAttached(object view, object context)
        {
            if (View == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    View = (ProjectDesignSurfaceView)view;
                    DesignSurfaceCanvas = (Canvas)projectDesignSurfaceView.FindName("DesignSurfaceCanvas");
                }
            }

            await GetCorpora();
            base.OnViewAttached(view, context);
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        protected override async void OnViewReady(object view)
        {
            await GetCorpora();
            base.OnViewReady(view);
        }

        private async Task GetCorpora()
        {
            // var corpora = await ProjectManager.LoadProject(ProjectManager.CurrentDashboardProject.ProjectName);
            //await EventAggregator.PublishOnUIThreadAsync(new CorporaLoadedMessage(corpora));

            // Corpora = new ObservableCollection<Corpus>(corpora);

        }

        public void AddManuscriptCorpus()
        {
            Logger.LogInformation("AddParatextCorpus called.");
        }

        public void AddUsfmCorpus()
        {
            Logger.LogInformation("AddParatextCorpus called.");
        }

        public async void AddParatextCorpus()
        {
            Logger.LogInformation("AddParatextCorpus called.");
            _addParatextCorpusRunning = true;
            var token = _tokenSource.Token;


            await ProjectManager.InvokeDialog<AddParatextCorpusDialogViewModel, AddParatextCorpusDialogViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<AddParatextCorpusDialogViewModel, Task<bool>>)Callback);

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {
                //_callBackRunning = true;
                if (viewModel.SelectedProject != null)
                {
                    var metadata = viewModel.SelectedProject;
                    await Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            CopyOriginalDatabase();
                            
                            
                            // if (viewModel.SelectedProject.HasProjectPath)
                            {
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating corpus '{metadata.Name}'...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                
                                var corpus = await Corpus.Create(ProjectManager.Mediator, metadata.IsRtl, metadata.Name!, metadata.LanguageName!, 
                                    metadata.CorpusTypeDisplay, token);
                                
                                OnUIThread(() => Corpora.Add(corpus));

                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Tokenizing and transforming '{metadata.Name}' corpus...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                
                                var textCorpus = (await ParatextProjectTextCorpus.Get(ProjectManager.Mediator, metadata.Id!, token))
                                    .Tokenize<LatinWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                
                                var tokenizedTextCorpus = await textCorpus.Create(ProjectManager.Mediator,corpus.CorpusId,
                                    ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", token);

                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Completed
                                }));
                                
                                Logger.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");
                                await EventAggregator.PublishOnCurrentThreadAsync(
                                    new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus, metadata));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex,$"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                            if (!token.IsCancellationRequested)
                            {
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                                    new BackgroundTaskStatus
                                    {
                                        Name = "Corpus",
                                        EndTime = DateTime.Now,
                                        ErrorMessage = $"{ex}",
                                        TaskStatus = StatusEnum.Error
                                    }));
                            }
                            else
                            {
                                RestoreOriginalDatabase();
                            }
                        }
                        finally
                        {
                            //_tokenSource.Dispose();
                            DeleteOriginalDatabase();
                            _addParatextCorpusRunning = false;
                        }
                    });
                }
                // We don't want to navigate anywhere.
                return false;
            }
        }
        private void DeleteOriginalDatabase()
        {
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dashboardPath = Path.Combine(documentsPath, $"ClearDashboard_Projects");
            try
            {
                File.Delete(Path.Combine(dashboardPath, $"{projectName}_original.sqlite"));
            }
            catch (Exception)
            {

            }
        }

        private void RestoreOriginalDatabase()
        {
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dashboardPath = Path.Combine(documentsPath, $"ClearDashboard_Projects");
            var projectPath = Path.Combine(dashboardPath, projectName);
            try
            {
                if (ProjectManager != null)
                {
                    ProjectManager.ProjectNameDbContextFactory.ProjectAssets.ProjectDbContext.Database.EnsureDeleted();
                }

                File.Move(
                    Path.Combine(dashboardPath, $"{projectName}_original.sqlite"),
                    Path.Combine(projectPath, $"{projectName}.sqlite"));
            }
            catch
            {

            }
        }

        private void CopyOriginalDatabase()
        {
            //make a copy of the database here named original_ProjectName.sqlite
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dashboardPath = Path.Combine(documentsPath, $"ClearDashboard_Projects");
            var projectPath = Path.Combine(dashboardPath, projectName);
            var filePath = Path.Combine(projectPath, $"{projectName}.sqlite");
            try
            {
                File.Copy(filePath, Path.Combine(dashboardPath, $"{projectName}_original.sqlite"));
            }
            catch
            {

            }
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;

            if (incomingMessage.Name == "Corpus" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
            {
                _tokenSource.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskStatus = StatusEnum.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage));
            }

            await Task.CompletedTask;
        }
    }
}
