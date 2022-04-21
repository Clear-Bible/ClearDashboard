using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ApplicationScreen : Screen, IDisposable
    {
        public ILogger Logger { get; private set; }
        public INavigationService NavigationService { get; private set; }
        public ProjectManager ProjectManager { get; private set; }

        private bool isBusy_;
        public bool IsBusy
        {
            get => isBusy_;
            set => Set(ref isBusy_, value, nameof(IsBusy));
        }

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set => Set(ref _flowDirection, value, nameof(FlowDirection));
        }

        public ApplicationScreen()
        {

        }

        public ApplicationScreen(INavigationService navigationService, ILogger logger, ProjectManager projectManager)
        {
            NavigationService = navigationService;
            Logger = logger;
            ProjectManager = projectManager;

            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose of unmanaged resources here...
            }
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public Task<TResponse> ExecuteCommand<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            IsBusy = true;
            try
            {
                return ProjectManager.ExecuteCommand(request, cancellationToken);
            }
            finally
            {
                IsBusy = false;
            }
        }
       
    }
}
