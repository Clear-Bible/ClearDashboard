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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Design;
using System.Windows.Threading;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
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
            var token = _tokenSource.Token;//this needs to be checked and thrown
            Logger.LogInformation("AddParatextCorpus called.");
            
            await ProjectManager.InvokeDialog<AddParatextCorpusDialogViewModel, AddParatextCorpusDialogViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<AddParatextCorpusDialogViewModel, Task<bool>>)Callback);

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {
                CopyOriginalDatabase();
                if (viewModel.SelectedProject != null)
                {
                    var metadata = viewModel.SelectedProject;
                    await Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            //await EventAggregator.PublishOnCurrentThreadAsync(
                            //    new ProgressBarVisibilityMessage(true));
                          

                           // if (viewModel.SelectedProject.HasProjectPath)
                            {
                                CheckAndThrowCancellationToken(token);
                                //await SendProgressBarMessage($"Creating corpus '{metadata.Name}'");
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating corpus '{metadata.Name}'...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                CheckAndThrowCancellationToken(token);
                                //***HERE IT DOES SOMETHING
                                var corpus = await Corpus.Create(ProjectManager.Mediator, metadata.IsRtl, metadata.Name,
                            metadata.LanguageName, metadata.CorpusTypeDisplay);
                                CheckAndThrowCancellationToken(token);
                                //await SendProgressBarMessage($"Created corpus '{metadata.Name}'");
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating corpus '{metadata.Name}'...Completed",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                CheckAndThrowCancellationToken(token);

                                //***HERE IT DOES SOMETHING
                                OnUIThread(() => Corpora.Add(corpus));
                                CheckAndThrowCancellationToken(token);

                                //await SendProgressBarMessage($"Tokenizing and transforming '{metadata.Name}' corpus.");
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Tokenizing and transforming '{metadata.Name}' corpus...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                CheckAndThrowCancellationToken(token);

                                //var textCorpus = new ParatextTextCorpus(metadata.ProjectPath)
                                //    .Tokenize<LatinWordTokenizer>()
                                //    .Transform<IntoTokensTextRowProcessor>();

                                //***HERE IT DOES SOMETHING
                                var textCorpus = (await ParatextProjectTextCorpus.Get(ProjectManager.Mediator, metadata.Id))
                                    .Tokenize<LatinWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                CheckAndThrowCancellationToken(token);
                                //await SendProgressBarMessage(
                                //    $"Completed Tokenizing and Transforming '{metadata.Name}' corpus.");
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Tokenizing and transforming '{metadata.Name}' corpus...Completed",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                CheckAndThrowCancellationToken(token);
                                

                                //await SendProgressBarMessage(
                                //    $"Creating tokenized text corpus for '{metadata.Name}' corpus.");
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));
                                CheckAndThrowCancellationToken(token);

                                //***HERE IT DOES SOMETHING
                                var tokenizedTextCorpus = await textCorpus.Create(ProjectManager.Mediator,
                            corpus.CorpusId,
                            ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
                                CheckAndThrowCancellationToken(token);
                                //await SendProgressBarMessage(
                                //    $"Completed creating tokenized text corpus for '{metadata.Name}' corpus.");
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Completed
                                }));
                                CheckAndThrowCancellationToken(token);
                                Logger.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");
                                await EventAggregator.PublishOnCurrentThreadAsync(
                                    new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus, metadata));
                                CheckAndThrowCancellationToken(token);
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
                            _tokenSource.Dispose();//this should hapen in a finallt, after the token has been thrown\
                            await EventAggregator.PublishOnCurrentThreadAsync(
                                new ProgressBarVisibilityMessage(false));
                        }

                    });
                }
                // We don't want to navigate anywhere.
                return false;
            }
        }

        private void RestoreOriginalDatabase()
        {
            //restore original database
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(documentsPath, $"ClearDashboard_Projects\\{projectName}");
            var filePath = Path.Combine(folderPath, $"{projectName}.sqlite");
            File.Delete(filePath);
            File.Move(
                Path.Combine(folderPath, $"{projectName}_original.sqlite"),
                Path.Combine(folderPath, $"{projectName}.sqlite"));
        }

        private void CopyOriginalDatabase()
        {
            //make a copy of the database here named original_ProjectName.sqlite
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(documentsPath, $"ClearDashboard_Projects\\{projectName}");
            var filePath = Path.Combine(folderPath, $"{projectName}.sqlite");
            File.Copy(filePath, Path.Combine(folderPath, $"{projectName}_original.sqlite"));
        }

        private static void CheckAndThrowCancellationToken(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;

            if (incomingMessage.Name == "Corpus" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
            {
                _tokenSource.Cancel();
               



                // cancel your task here AddParatextCorpus()
                //FIGURE OUT HOW TO CANCEL TASK and reverse it's changes
                    //keep track of waht it is doing
                        //keep a stack of what has been accomplised in the task
                            //things made
                            //things deleted
                    //when in the process it is being canceled (what has it done so far)
                    //undo what it has done
                        //go reverse in the stack of it's actions

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
