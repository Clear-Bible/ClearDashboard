using Caliburn.Micro;
//using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ClearDashboard.Wpf.ViewModels.Project
{

    public record CorporaLoadedMessage(IEnumerable<Corpus> Copora);

    public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus);

    public class ProjectDesignSurfaceViewModel : ToolViewModel
    {
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
            Logger.LogInformation("AddParatextCorpus called.");

            await ProjectManager.InvokeDialog<AddParatextCorpusDialogViewModel, AddParatextCorpusDialogViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<AddParatextCorpusDialogViewModel, Task<bool>>)Callback);

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {

                if (viewModel.SelectedProject != null)
                {
                    var metadata = viewModel.SelectedProject;
                    await Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            await EventAggregator.PublishOnCurrentThreadAsync(
                                new ProgressBarVisibilityMessage(true));
                          

                            if (viewModel.SelectedProject.HasProjectPath)
                            {

                                await SendProgressBarMessage($"Creating corpus '{metadata.Name}'");

                                var corpus = await Corpus.Create(ProjectManager.Mediator, metadata.IsRtl, metadata.Name,
                                    metadata.LanguageName, metadata.CorpusTypeDisplay);
                                await SendProgressBarMessage($"Created corpus '{metadata.Name}'");

                                OnUIThread(() => Corpora.Add(corpus));

                                await SendProgressBarMessage($"Tokenizing and transforming '{metadata.Name}' corpus.");
                                var textCorpus = new ParatextTextCorpus(metadata.ProjectPath)
                                    .Tokenize<LatinWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                await SendProgressBarMessage(
                                    $"Completed Tokenizing and Transforming '{metadata.Name}' corpus.");


                                await SendProgressBarMessage(
                                    $"Creating tokenized text corpus for '{metadata.Name}' corpus.");
                                var tokenizedTextCorpus = await textCorpus.Create(ProjectManager.Mediator,
                                    corpus.CorpusId,
                                    ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
                                await SendProgressBarMessage(
                                    $"Completed creating tokenized text corpus for '{metadata.Name}' corpus.");

                                Logger.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");
                                await EventAggregator.PublishOnCurrentThreadAsync(
                                    new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex,$"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                        }
                        finally
                        {
                            await EventAggregator.PublishOnCurrentThreadAsync(
                                new ProgressBarVisibilityMessage(false));
                        }

                    });
                }
                // We don't want to navigate anywhere.
                return false;
            }
        }
    }
}
