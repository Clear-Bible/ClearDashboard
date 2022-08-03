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
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
//using SIL.Extensions;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ClearDashboard.Wpf.ViewModels.Project
{

    public record CorporaLoadedMessage(IEnumerable<Corpus> Copora);

    public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus);

    public class ProjectDesignSurfaceViewModel : ToolViewModel
    {
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

        public ProjectDesignSurfaceViewModel(IMediator mediator, INavigationService navigationService, ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            _mediator = mediator;
            Title = "PROJECT DESIGN SURFACE";
            ContentId = "PROJECTDESIGNSURFACETOOL";

            Corpora = new ObservableCollection<Corpus>();
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }
        
        public ProjectDesignSurfaceView View { get; set; }
        public Canvas DesignSurfaceCanvas { get; set; }
        protected override async  void OnViewAttached(object view, object context)
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

        protected override async  void OnViewReady(object view)
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

            await (Parent as ProjectWorkspaceViewModel).ActiveAlignmentView();

            await ProjectManager.InvokeDialog<AddParatextCorpusDialogViewModel, AddParatextCorpusDialogViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<AddParatextCorpusDialogViewModel, Task<bool>>)Callback);

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {
                //OnUIThread(async () =>
                //{
                if (viewModel.SelectedProject != null)
                {
                    //IsBusy = true;

                    //try
                    //{
                    //Task.Factory.StartNew(() => { //long task });
                    var metadata = viewModel.SelectedProject;

                    if (viewModel.SelectedProject.HasProjectPath)
                    {
                        //ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
                        // into TokensTextRow, puts them into the DB, and returns a TokensTextRow.

                        Logger.LogInformation($"Creating corpus '{metadata.Name}");
                        var corpus = await Corpus.Create(ProjectManager.Mediator, metadata.IsRtl, metadata.Name,
                            metadata.LanguageName, metadata.CorpusTypeDisplay);

                        OnUIThread(()=> Corpora.Add(corpus));
                        

                        Logger.LogInformation($"Tokenizing and transforming {metadata.Name} corpus.");
                        var textCorpus = new ParatextTextCorpus(metadata.ProjectPath)
                            .Tokenize<LatinWordTokenizer>()
                            .Transform<IntoTokensTextRowProcessor>();
                        Logger.LogInformation($"Completed Tokenizing and Transforming {metadata.Name} corpus.");


                        Logger.LogInformation($"Creating tokenized text corpus for {metadata.Name} corpus.");
                        var tokenizedTextCorpus = await textCorpus.Create(ProjectManager.Mediator, corpus.CorpusId,
                            ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
                        Logger.LogInformation($"Completed creating tokenized text corpus for {metadata.Name} corpus.");

                        await EventAggregator.PublishOnUIThreadAsync(
                            new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus));
                    }

                    //}
                    //finally
                    //{
                    //    IsBusy = false;
                    //}
                }
                // });
                // We don't want to navigate anywhere.
                return false;
            }
        }
    }
}
