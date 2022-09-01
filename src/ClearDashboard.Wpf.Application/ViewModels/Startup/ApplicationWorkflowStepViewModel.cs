using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public abstract class ApplicationWorkflowStepViewModel: DashboardApplicationScreen, IWorkflowStepViewModel
    {
        #region Member Variables
        private Direction _direction;

        private bool _enableControls;

        private bool _canMoveForwards;

        private bool _canMoveBackwards;

        private bool _showBackButton;

        private bool _showForwardButton;
        #endregion

        #region Properties
        public Direction Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                Set(ref _direction, value, "Direction");
            }
        }

        public bool EnableControls
        {
            get
            {
                return _enableControls;
            }
            set
            {
                ILogger? logger = base.Logger;
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
                defaultInterpolatedStringHandler.AppendLiteral("WorkflowStepViewModel - Setting EnableControls to ");
                defaultInterpolatedStringHandler.AppendFormatted(value);
                defaultInterpolatedStringHandler.AppendLiteral(" at ");
                defaultInterpolatedStringHandler.AppendFormatted(DateTime.Now, "HH:mm:ss.fff");
                logger.LogInformation(defaultInterpolatedStringHandler.ToStringAndClear());
                Set(ref _enableControls, value, "EnableControls");
            }
        }

        public bool CanMoveForwards
        {
            get
            {
                return _canMoveForwards;
            }
            set
            {
                Set(ref _canMoveForwards, value, "CanMoveForwards");
            }
        }

        public bool CanMoveBackwards
        {
            get
            {
                return _canMoveBackwards;
            }
            set
            {
                Set(ref _canMoveBackwards, value, "CanMoveBackwards");
            }
        }

        public bool ShowBackButton
        {
            get
            {
                return _showBackButton;
            }
            set
            {
                Set(ref _showBackButton, value, "ShowBackButton");
            }
        }

        public bool ShowForwardButton
        {
            get
            {
                return _showForwardButton;
            }
            set
            {
                Set(ref _showForwardButton, value, "ShowForwardButton");
            }
        }
        #endregion

        #region Constructor
        protected ApplicationWorkflowStepViewModel()
        {
        }

        public ApplicationWorkflowStepViewModel (DashboardProjectManager projectManager,
            INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {

        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ShowWorkflowButtons();
            EnableControls = (Parent as WorkflowShellViewModel).EnableControls;
            return base.OnActivateAsync(cancellationToken);
        }
        #endregion

        #region Methods
        public virtual async Task MoveForwardsAction()
        {
            await Task.CompletedTask;
        }

        public virtual async Task MoveBackwardsAction()
        {
            await Task.CompletedTask;
        }

        public async Task MoveForwards()
        {
            Direction = Direction.Forwards;
            await MoveForwardsAction();
            await TryCloseAsync();
        }

        public async Task MoveBackwards()
        {
            Direction = Direction.Backwards;
            await MoveBackwardsAction();
            await TryCloseAsync();
        }

        protected void ShowWorkflowButtons()
        {
            List<IWorkflowStepViewModel> list = (Parent as WorkflowShellViewModel)?.Steps;
            if (list != null)
            {
                int num = list.IndexOf(this);
                if (num == 0 && list.Count == 1)
                {
                    ShowBackButton = false;
                    ShowForwardButton = false;
                }

                if (num > 0 && num < list.Count - 1)
                {
                    ShowBackButton = true;
                    ShowForwardButton = true;
                }

                if (num == 0 && list.Count > 1)
                {
                    ShowBackButton = false;
                    ShowForwardButton = true;
                }

                if (list.Count > 1 && num == list.Count - 1)
                {
                    ShowBackButton = true;
                    ShowForwardButton = false;
                }
            }
        }
        #endregion
    }
}
