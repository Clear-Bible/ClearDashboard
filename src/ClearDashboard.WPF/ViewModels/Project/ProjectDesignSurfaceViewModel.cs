using AvalonDock.Controls;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using ClearDashboard.Wpf.Views;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class ProjectDesignSurfaceViewModel : ToolViewModel
    {
        private ObservableCollection<Corpus> _corpora;
        public ObservableCollection<Corpus> Corpora
        {
            get => _corpora;
            set => _corpora = value;
        }

        public ProjectDesignSurfaceViewModel()
        {

        }

        public ProjectDesignSurfaceViewModel(INavigationService navigationService, ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
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
        protected override void OnViewAttached(object view, object context)
        {
            if (View == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    View = (ProjectDesignSurfaceView)view;
                    DesignSurfaceCanvas = (Canvas)projectDesignSurfaceView.FindName("DesignSurfaceCanvas");
                }
                base.OnViewAttached(view, context);
            }
        }

        protected override async void OnViewLoaded(object view)
        {
          
            var project = await ProjectManager.LoadProject(ProjectManager.CurrentDashboardProject.ProjectName);
            
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }

        private void DrawCopora()
        {

            OnUIThread(() =>
            {
                DesignSurfaceCanvas.Children.Clear();
                var index = 0;
                foreach (var corpus in Corpora)
                {
                    var border = CreateCorpusDisplay(corpus);

                    if (index <= 3)
                    {
                        Canvas.SetTop(border, 10.0);
                    }
                    else
                    {
                        Canvas.SetTop(border, 85.0);
                    }

                    Canvas.SetLeft(border, 10 + (index * 200));
                    DesignSurfaceCanvas.Children.Add(border);

                    index++;

                }
            });
        }

        private static Border CreateCorpusDisplay(Corpus corpus)
        {
            var border = new Border
            {
                Height = 75,
                Width = 150,
                Background = Brushes.Blue,
                CornerRadius = new CornerRadius(5)
            };

            var textBlock = new TextBlock
            {
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = corpus.Name,
            };

            border.Child = textBlock;
            return border;
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

            // Define a callback method to create a new project if we
            // have a valid project name

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {
                if (viewModel.SelectedProject != null)
                {
                    //await ProjectManager.CreateNewProject(viewModel.ProjectName);
                    //return true;

                    // TODO:
                    // Add Corpus
                    // Get Books from Paratext
                    var corpus = new Corpus
                    {
                        Name = viewModel.SelectedProject.Name,
                        Language = viewModel.SelectedProject.LanguageName,
                        CorpusType = viewModel.SelectedProject.CorpusType,

                    };

                    Corpora.Add(corpus);
                    DrawCopora();

                }

                // We don;t want to navigate anywhere.
                return false;
            }
        }
    }
}
