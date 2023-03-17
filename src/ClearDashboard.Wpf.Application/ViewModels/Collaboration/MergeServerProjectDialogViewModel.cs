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
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CollaborationManager _collaborationManager;
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
                CanAction = !string.IsNullOrEmpty(value);
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
                ProgressBarVisibility = Visibility.Visible;

                _runningTask = Task.Run(() => _collaborationManager.InitializeProjectDatabaseAsync(ProjectId, true, _cancellationTokenSource.Token, new Progress<ProgressStatus>(Report)));
                await _runningTask;

                await TryCloseAsync(true);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                // FIXME:  log exception?  throw?  some other type of close?
                await TryCloseAsync(false);
            }
            finally
            {
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
                ProgressBarVisibility = Visibility.Visible;

                _runningTask = Task.Run(() => _collaborationManager.MergeProjectLatestChangesAsync(true, false, _cancellationTokenSource.Token, new Progress<ProgressStatus>(Report)));
                await _runningTask;

                await TryCloseAsync(true);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                // FIXME:  log exception?  throw?  some other type of close?
                await TryCloseAsync(false);
            }
            finally
            {
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
        public string DialogTitle => $"{Action} Server Project: {ProjectName}";

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
                _cancellationTokenSource.Cancel();
                await Task.WhenAny(tasks: new Task[] { _runningTask, Task.Delay(30000) });
            }

            await TryCloseAsync(false);
        }

        public string Action => IsImportAction ? "Import" : "Merge";
        public string ProgressLabel => $"{Action} Progress";
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

        private bool _canAction;
        public bool CanAction
        {
            get => _canAction;
            set => Set(ref _canAction, value);
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
