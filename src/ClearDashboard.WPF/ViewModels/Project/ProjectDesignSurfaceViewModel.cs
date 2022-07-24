using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.ViewModels.Popups;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class ProjectDesignSurfaceViewModel : ToolViewModel
    {
        private ObservableCollection<Corpus> _copora;
        public ObservableCollection<Corpus> Copora
        {
            get => _copora;
            set => _copora = value;
        }

        public ProjectDesignSurfaceViewModel()
        {

        }

        public ProjectDesignSurfaceViewModel(INavigationService navigationService, ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "PROJECT DESIGN SURFACE";
            ContentId = "PROJECTDESIGNSURFACETOOL";
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Copora = new ObservableCollection<Corpus>();
            return base.OnInitializeAsync(cancellationToken);
        }


        private Canvas _designSurfaceCanvas;
        protected override void OnViewAttached(object view, object context)
        {
            if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
            {
                _designSurfaceCanvas = (Canvas)projectDesignSurfaceView.FindName("DesignSurface");
            }

            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            DrawCopora();
            base.OnViewLoaded(view);
        }

        private void DrawCopora()
        {
            _designSurfaceCanvas.Children.Clear();
            foreach (var corpus in Copora)
            {
                var rectangle = new Rectangle()
                {
                    //Content = corpus.Name,
                    Width = 300,
                    Height = 100,
                    Fill = Application.Current.FindResource("OrangeMidBrush") as Brush,
                    RadiusX = 3,
                    RadiusY = 3,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    }



                };
                Canvas.SetTop(rectangle, 10.0);
                Canvas.SetLeft(rectangle, 10.0);
                _designSurfaceCanvas.Children.Add(rectangle);
            }
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

                    Copora.Add(corpus);
                    DrawCopora();

                }

                // We don;t want to navigate anywhere.
                return false;
            }
        }
    }
}
