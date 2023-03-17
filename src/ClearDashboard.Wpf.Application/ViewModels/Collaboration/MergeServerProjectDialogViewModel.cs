using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.DataAccessLayer.Models.Common;
using System.Collections.ObjectModel;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Services;
using System.Windows.Threading;
using System;
using System.Dynamic;
using System.Windows.Controls.Primitives;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using SIL.Machine.Utils;
using System.Text.RegularExpressions;

namespace ClearDashboard.Wpf.Application.ViewModels.Collaboration
{
    public class MergeServerProjectDialogViewModel : DashboardApplicationScreen
    {
        private readonly CollaborationManager _collaborationManager;
        private CancellationTokenSource _cancellationTokenSource;
        private Task? _runningTask;

        private bool _isImportAction = true;
        public bool IsImportAction
        {
            get => _isImportAction;
            set => _isImportAction = value;
        }

        private Guid _projectId = Guid.Empty;
        public Guid ProjectId
        {
            get => _projectId;
            set =>_projectId = value;
        }

        private string _projectName = string.Empty;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                Set(ref _projectName, value);
                ProjectManager!.CurrentDashboardProject.ProjectName = value;
                CanOkAction = !string.IsNullOrEmpty(value);
                NotifyOfPropertyChange(nameof(Project));
            }
        }
        private Visibility? _progressBarVisibility = Visibility.Hidden;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        public MergeServerProjectDialogViewModel(CollaborationManager collaborationManager, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _collaborationManager = collaborationManager;

            //return base.OnInitializeAsync(cancellationToken);
        }

        public async Task Import()
        {
            if (ProjectId == Guid.Empty)
            {
                return;
            }

            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            try
            {
                CanOkAction = false;
                CancelAction = "Cancel";
                ProgressBarVisibility = Visibility.Visible;

                MergeProgressUpdates.Clear();
                if (!_cancellationTokenSource.TryReset())
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                _runningTask = Task.Run(() => _collaborationManager.InitializeProjectDatabaseAsync(ProjectId, true, _cancellationTokenSource.Token, new Progress<ProgressStatus>(Report)));
                await _runningTask;

                // If Import succeeds, don't set CanAction back to true
            }
            catch (Exception)
            {
                CanOkAction = true;
            }
            finally
            {
                _runningTask = null;

                CancelAction = "Close";
                ProgressBarVisibility = Visibility.Hidden;
            }
        }

        public async Task Merge()
        {
            if (ProjectId == Guid.Empty)
            {
                return;
            }

            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            try
            {
                CanOkAction = false;
                CancelAction = "Cancel";
                ProgressBarVisibility = Visibility.Visible;

                MergeProgressUpdates.Clear();
                if (!_cancellationTokenSource.TryReset())
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                _runningTask = Task.Run(() => _collaborationManager.MergeProjectLatestChangesAsync(true, false, _cancellationTokenSource.Token, new Progress<ProgressStatus>(Report)));
                await _runningTask;
            }
            finally
            {

                _runningTask = null;

                CanOkAction = true;
                CancelAction = "Close";
                ProgressBarVisibility = Visibility.Hidden;
            }
        }

        private bool CheckIfConnectedToParatext()
        {
            if (ProjectManager?.HasCurrentParatextProject == false)
            {
                return false;
            }
            return true;
        }
        public string DialogTitle => $"{OkAction} Server Project: {ProjectName}";

        public dynamic DialogSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.SingleBorderWindow;
            settings.ShowInTaskbar = false;
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.Placement = PlacementMode.Center;
            settings.Width = 600;
            settings.Height = 800;
            settings.Title = DialogTitle;
            return settings;
        }

        public bool CanCancel => true /* can always cancel */;
        public async Task Cancel()
        {
            if (_runningTask is not null)
            {
                CanCancelAction = false;
                try
                {
                    _cancellationTokenSource.Cancel();
                    await Task.WhenAny(tasks: new Task[] { _runningTask, Task.Delay(30000) });
                }
                finally
                {
                    CanCancelAction = true;
                }
            }
            else
            {
                await TryCloseAsync(false);
            }
        }

        public async Task Ok()
        {
            if (IsImportAction)
            {
                await Import();
            } 
            else 
            { 
                await Merge(); 
            }
         }

        public string ProgressLabel => $"{OkAction} Progress";
        public string OkAction => IsImportAction ? "Import" : "Merge";

        private bool _canOkAction;
        public bool CanOkAction
        {
            get => _canOkAction;
            set => Set(ref _canOkAction, value);
        }

        private bool _canCancelAction = true;
        public bool CanCancelAction
        {
            get => _canCancelAction;
            set => Set(ref _canCancelAction, value);
        }

        private string _cancelAction = "Close";
        public string CancelAction
        {
            get => _cancelAction;
            set => Set(ref _cancelAction, value);
        }

        public void Report(ProgressStatus status)
        {
            var message = Regex.Replace(status.Message ?? string.Empty, "{PercentCompleted(:.*)?}", "{0$1}");
            var description = Regex.IsMatch(message, "{0(:.*)?}") ?
                string.Format(message, status.PercentCompleted) :
                message;

            System.Windows.Application.Current.Dispatcher.Invoke(() => MergeProgressUpdates.Add(description));
        }

        private ObservableCollection<string> _mergeProgressUpdates = new();
        public ObservableCollection<string> MergeProgressUpdates
        {
            get => _mergeProgressUpdates;
            set
            {
                _mergeProgressUpdates = value;
                NotifyOfPropertyChange(() => MergeProgressUpdates);
            }
        }
    }
}
