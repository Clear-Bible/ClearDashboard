using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class ProjectWorkspaceWithGridSplitterViewModel : Conductor<Screen>.Collection.OneActive
    {

        private IEventAggregator EventAggregator { get; }
        private DashboardProjectManager ProjectManager { get; }
        private ILogger<ProjectWorkspaceWithGridSplitterViewModel> Logger { get; }
        private INavigationService NavigationService { get; }
        private DashboardProject DashboardProject { get; }

        public ProjectDesignSurfaceViewModel ProjectDesignSurfaceViewModel { get; set; }

        /// <summary>
        /// Required for design-time support
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public ProjectWorkspaceWithGridSplitterViewModel()
        {

        }

        // ReSharper disable once UnusedMember.Global
        public ProjectWorkspaceWithGridSplitterViewModel(INavigationService navigationService,
            ILogger<ProjectWorkspaceWithGridSplitterViewModel> logger, DashboardProjectManager projectManager,
            IEventAggregator eventAggregator)

        {
            EventAggregator = eventAggregator;
            ProjectManager = projectManager;
            Logger = logger;
            NavigationService = navigationService;
            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;

            ProjectDesignSurfaceViewModel = IoC.Get<ProjectDesignSurfaceViewModel>();

        }

        private readonly FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;

        public FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            init => Set(ref _windowFlowDirection, value, nameof(WindowFlowDirection));
        }

        private async Task ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : Screen
        {
            var viewModel = IoC.Get<TViewModel>();
            viewModel.Parent = this;
            viewModel.ConductWith(this);
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            // subscribe to the event aggregator so that we can listen to messages
            EventAggregator.SubscribeOnUIThread(this);

            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            // unsubscribe to the event aggregator
            EventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await Initialize();
            await base.OnInitializeAsync(cancellationToken);
        }


        private Grid ProjectGrid { get; set; }
        protected override async void OnViewReady(object view)
        {
            //ProjectGrid = (Grid) ((ProjectWorkspaceWithGridSplitterView)view).FindName("ProjectGrid");
            //if (ProjectGrid != null)
            //{
            //    var designSurfaceView = ProjectDesignSurfaceViewModel.View;
            //    Grid.SetColumn(designSurfaceView, 0);
            //    Grid.SetRow(designSurfaceView, 0);
            //    ProjectGrid.Children.Add(designSurfaceView);
            //}
            base.OnViewReady(view);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }


        private async Task Initialize()
        {
            Items.Clear();

            //await ActivateDesignSurface();
            // documents
            //await ActivateItemAsync<AlignmentViewModel>();
            //await ActivateItemAsync<CorpusViewModel>();

            // tools
            //await ActivateItemAsync<ProjectDesignSurfaceViewModel>();

        }

        private async Task ActivateDesignSurface()
        {
           
            //var view = ViewLocator.LocateForModel(ProjectDesignSurfaceViewModel, null, null);
            //ViewModelBinder.Bind(ProjectDesignSurfaceViewModel, view, null); 
            //await ProjectDesignSurfaceViewModel.ActivateAsync();

        }
    }
}
