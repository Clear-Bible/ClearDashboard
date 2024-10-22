﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;

namespace ClearDashboard.Wpf.Application.ViewModels.Collaboration
{
    public enum CollaborationDialogAction
    {
        Import,
        Merge,
        Commit,
        Initialize
    }

    public class MergeServerProjectDialogViewModel : DashboardApplicationScreen
    {
        #region Member Variables   

        private readonly CollaborationManager _collaborationManager;
        private readonly ILogger<ProjectSetupViewModel> _logger;
        private readonly GitLabHttpClientServices _gitLabHttpClientServices;
        private CancellationTokenSource _cancellationTokenSource;
        private Task? _runningTask;

        private CollaborationConfiguration _userInfo;
        private GitLabUser _gitLabUser;

        private ILocalizationService _localizationService;
        private string DialogTitle => $"{OkAction}: {ProjectName}";
        private bool _completedTask = false;

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        public string ProgressLabel => $"{OkAction} Progress";

        private CollaborationDialogAction _dialogAction = CollaborationDialogAction.Import;
        public CollaborationDialogAction CollaborationDialogAction
        {
            get => _dialogAction;
            set => _dialogAction = value;
        }

        private Guid _projectId = Guid.Empty;
        public Guid ProjectId
        {
            get => _projectId;
            set => _projectId = value;
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

        public string CommitMessage { get; set; } = "Commit issued";  // Set by MainViewModel
        public string? CommitSha { get; private set; }

        private Visibility? _progressBarVisibility = Visibility.Hidden;

        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        public bool CanCancel => true /* can always cancel */;


        public string OkAction
        {
            get
            {
                switch (CollaborationDialogAction)
                {
                    case CollaborationDialogAction.Commit:
                        return "Send Changes";
                    case CollaborationDialogAction.Import:
                        return CollaborationDialogAction.ToString();
                    case CollaborationDialogAction.Initialize:
                        return "Make Project Available for Collab";
                    case CollaborationDialogAction.Merge:
                        return "Get Latest Updates";
                    default:
                        return CollaborationDialogAction.ToString();
                }
            }
        }

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

        private string _cancelAction;
        public string CancelAction
        {
            get => _cancelAction;
            set
            {
                _cancelAction = value;
                NotifyOfPropertyChange(() => CancelAction);
            }
        }

        private System.Windows.Media.Brush _cancelColor;
        public System.Windows.Media.Brush CancelColor
        {
            get => _cancelColor;
            set
            {
                _cancelColor = value;
                NotifyOfPropertyChange(() => CancelColor);
            }
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


        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }


        private System.Windows.Media.Brush statusMessageColor;
        public System.Windows.Media.Brush StatusMessageColor 
        { 
            get => statusMessageColor; 
            set => Set(ref statusMessageColor, value); 
        }

        #endregion //Observable Properties


        #region Constructor

        public MergeServerProjectDialogViewModel()
        {
            // no-op used for Caliburn Micro
        }

        public MergeServerProjectDialogViewModel(CollaborationManager collaborationManager,
            DashboardProjectManager projectManager,
            INavigationService navigationService,
            ILogger<ProjectSetupViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            TranslationSource translationSource,
            GitLabHttpClientServices gitLabHttpClientServices,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _localizationService = localizationService;
            _cancellationTokenSource = new CancellationTokenSource();
            _collaborationManager = collaborationManager;
            _logger = logger;
            _gitLabHttpClientServices = gitLabHttpClientServices;

            //return base.OnInitializeAsync(cancellationToken);
        }


        protected override async void OnViewLoaded(object view)
        {
            _userInfo = _collaborationManager.GetConfig();
            _gitLabUser = new Models.HttpClientFactory.GitLabUser
            {
                Id = _userInfo.UserId,
                Email = _userInfo.RemoteEmail,
                UserName = _userInfo.RemoteUserName,
                NamespaceId = _userInfo.NamespaceId,
                Organization = _userInfo.Group,
                TokenId = _userInfo.TokenId
            };

            var projectCreated = await CreateProjectOnServerIfNotCreated();

            if (projectCreated == false)
            {
                // an existing project so we need to check to see if the user is a member of the project


                var projects = await _gitLabHttpClientServices.GetProjectsForUser(_userInfo);

                if (ProjectId != Guid.Empty)
                {
                    var currentProjectId = "P_" + ProjectId;
                    var project = projects.FirstOrDefault(x => x.Name == currentProjectId);

                    if (project is null)
                    {
                        _logger.LogInformation($"Project {currentProjectId} not found");
                        _logger.LogInformation($"Projects Count: {projects.Count}");
                        _logger.LogInformation($"CurrentProjectId: {currentProjectId}");

                        int i = 0;
                        foreach (var p in projects)
                        {
                            _logger.LogInformation($"Project {i} Name: {p.Name}");
                            i++;
                        }

                        StatusMessage = "User is not a member of the project.\nPlease contact the project owner to be added to the project.";
                        StatusMessageColor = Brushes.Red;
                        CancelAction = "Close";

                        await EventAggregator.PublishOnUIThreadAsync(new DashboardProjectPermissionLevelMessage(PermissionLevel.None));
                        return;
                    }
                }

            }

            Ok();  // run the action - do not await

            base.OnViewLoaded(view);
        }

        protected override async Task<Task> OnDeactivateAsync(bool close, CancellationToken cancellationToken)
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

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion //Constructor


        #region Methods

        private async Task<bool> CreateProjectOnServerIfNotCreated()
        {
            bool projectCreated = false;

            var projects = await _gitLabHttpClientServices.GetProjectsForUser(_userInfo);
            var project = projects.FirstOrDefault(x => x.Name == $"P_{ProjectId}");

            if (project is null)
            {
                projectCreated = true;
                project =
                    await _gitLabHttpClientServices.CreateNewProjectForUser(_gitLabUser, $"P_{ProjectId}", ProjectName);
            }

            _userInfo.RemoteUrl = project.HttpUrlToRepo.Replace("http:", "https:");
            _collaborationManager.SetRemoteUrl(_userInfo.RemoteUrl, project.Name);

            if (!_collaborationManager.IsRepositoryInitialized())
            {
                _collaborationManager.InitializeRepository();
            }

            try
            {
                _collaborationManager.FetchMergeRemote();
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, "Unable to fetch from server");
            }

            return projectCreated;
        }


        private bool PreAction()
        {
            StatusMessage = string.Empty;


            if (ProjectId == Guid.Empty)
            {
                return false;
            }

            if (CollaborationDialogAction != CollaborationDialogAction.Merge)
            {
                if (CheckIfConnectedToParatext() == false)
                {
                    return false;
                }
            }

            CanOkAction = false;
            CancelAction = "Cancel";
            ProgressBarVisibility = Visibility.Visible;

            if (!_cancellationTokenSource.TryReset())
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            return true;
        }

        private void PostAction()
        {
            _runningTask = null;
            
            ProgressBarVisibility = Visibility.Hidden;
        }

        private async Task Import()
        {
            _completedTask = false;
            IProgress<ProgressStatus> progress = new Progress<ProgressStatus>(Report);
            try
            {
                if (!PreAction())
                {
                    return;
                }

                _runningTask = Task.Run(async () => CommitSha = await _collaborationManager.InitializeProjectDatabaseAsync(
                    ProjectId,
                    true,
                    _cancellationTokenSource.Token,
                    progress));
                await _runningTask;

                progress.Report(new ProgressStatus(0, "Operation Finished!"));
                PlaySound.PlaySoundFromResource();

                _completedTask = true;

                StatusMessage = _localizationService["MergeDialog_OperationComplete"];
                StatusMessageColor = System.Windows.Media.Brushes.Green;
                CancelAction = "Done";
            }
            catch (OperationCanceledException)
            {
                progress.Report(new ProgressStatus(0, "Operation Cancelled"));
                PlaySound.PlaySoundFromResource(SoundType.Error);

                StatusMessage = _localizationService["MergeDialog_OperationCancelled"];
                StatusMessageColor = System.Windows.Media.Brushes.Orange;
                CancelAction = "Close";
            }
            catch (Exception ex)
            {
                progress.Report(new ProgressStatus(0, $"Exception thrown attempting to initialize project database: {ex.Message}"));
                CanOkAction = true;
                PlaySound.PlaySoundFromResource(SoundType.Error);

                StatusMessage = _localizationService["MergeDialog_OperationErrored"]; 
                StatusMessageColor = System.Windows.Media.Brushes.Red;
                CancelAction = "Close";
            }
            finally
            {
                // If Import succeeds, don't set CanAction back to true
                PostAction();
            }
        }

        private async Task Merge()
        {
            IProgress<ProgressStatus> progress = new Progress<ProgressStatus>(Report);
            try
            {
                if (!PreAction())
                {
                    return;
                }

                _runningTask = Task.Run(async () =>
                {

                    progress.Report(new ProgressStatus(0, "Fetching latest changes from server"));
                    _collaborationManager.FetchMergeRemote();

                    CommitSha = await _collaborationManager.MergeProjectLatestChangesAsync(
                        MergeMode.RemoteOverridesLocal,
                        false,
                        _cancellationTokenSource.Token,
                        progress);

                });
                await _runningTask;

                if (CommitSha is not null)
                {
                    await EventAggregator.PublishOnUIThreadAsync(new ReloadProjectMessage());
                    progress.Report(new ProgressStatus(0, "UI Reload Complete"));
                }

                progress.Report(new ProgressStatus(0, "Operation Finished!"));
                PlaySound.PlaySoundFromResource();
                
                StatusMessage = _localizationService["MergeDialog_OperationComplete"];
                StatusMessageColor = System.Windows.Media.Brushes.Green;
                CancelAction = "Done";

                await EventAggregator.PublishOnUIThreadAsync(new RefreshCheckGitLab(), CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                progress.Report(new ProgressStatus(0, "Operation Cancelled"));
                PlaySound.PlaySoundFromResource(SoundType.Error);

                StatusMessage = _localizationService["MergeDialog_OperationCancelled"];
                StatusMessageColor = System.Windows.Media.Brushes.Orange;
                CancelAction = "Close";
            }
            catch (Exception ex)
            {
                progress.Report(new ProgressStatus(0, $"Exception thrown attempting to merge latest project changes: {ex.Message}"));
                PlaySound.PlaySoundFromResource(SoundType.Error);

                StatusMessage = _localizationService["MergeDialog_OperationErrored"];
                StatusMessageColor = System.Windows.Media.Brushes.Red;
                CancelAction = "Close";
            }
            finally
            {
                CanOkAction = true;
                PostAction();
            }
        }

        private async Task Commit()
        {
            IProgress<ProgressStatus> progress = new Progress<ProgressStatus>(Report);
            try
            {
                if (!PreAction())
                {
                    return;
                }

                _runningTask = Task.Run(async () =>
                {

                    if (CollaborationDialogAction == CollaborationDialogAction.Initialize)
                    {
                        progress.Report(new ProgressStatus(0, "Initializing Collaboration Repository"));
                        _collaborationManager.InitializeRepository();
                    }

                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    progress.Report(new ProgressStatus(0, "Fetching Server Data"));
                    _collaborationManager.FetchMergeRemote();

                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    if (_collaborationManager.IsCurrentProjectInRepository())
                    {
                        var lastMergedCommitSha = await _collaborationManager.MergeProjectLatestChangesAsync(
                            MergeMode.RemoteOverridesLocal,
                            false,
                            _cancellationTokenSource.Token,
                            progress);

                        if (lastMergedCommitSha != null)
                        {
                            await EventAggregator.PublishOnUIThreadAsync(new ReloadProjectMessage());
                        }
                    }
                    await _collaborationManager.StageProjectChangesAsync(_cancellationTokenSource.Token, progress);

                    CommitSha = _collaborationManager.CommitChanges(CommitMessage, progress);

                    if (CommitSha is not null)
                    {
                        progress.Report(new ProgressStatus(0, "Pushing changes to remote"));
                        _collaborationManager.PushChangesToRemote();
                    }

                    progress.Report(new ProgressStatus(0, "Commit Complete!"));

                    progress.Report(new ProgressStatus(0, "Operation Finished!"));
                    PlaySound.PlaySoundFromResource();

                    StatusMessage = _localizationService["MergeDialog_OperationComplete"];
                    StatusMessageColor = System.Windows.Media.Brushes.Green;
                    CancelAction = "Done";
                });

                await _runningTask;

                await EventAggregator.PublishOnUIThreadAsync(new DashboardProjectPermissionLevelMessage(PermissionLevel.Owner));
            }
            catch (OperationCanceledException)
            {
                progress.Report(new ProgressStatus(0, "Operation Cancelled"));
                PlaySound.PlaySoundFromResource(SoundType.Error);

                StatusMessage = _localizationService["MergeDialog_OperationCancelled"];
                StatusMessageColor = System.Windows.Media.Brushes.Orange;
                CancelAction = "Close";
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("status code: 404"))
                {
                    progress.Report(new ProgressStatus(0, $"The project you are pushing changes to either no longer exists or is not shared with you."));
                }
                else if (CollaborationDialogAction == CollaborationDialogAction.Initialize)
                {
                    progress.Report(new ProgressStatus(0, $"Exception thrown attempting to initialize, fetch, stage and commit project changes: {ex.Message}"));
                }
                else
                {
                    progress.Report(new ProgressStatus(0, $"Exception thrown attempting to stage and commit project changes: {ex.Message}"));
                }
                PlaySound.PlaySoundFromResource(SoundType.Error);

                StatusMessage = _localizationService["MergeDialog_OperationErrored"];
                StatusMessageColor = System.Windows.Media.Brushes.Red;
                CancelAction = "Close";
            }
            finally
            {
                // If Commit succeeds, don't set CanAction back to true
                PostAction();
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
            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;
            return settings;
        }

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
                await TryCloseAsync(_completedTask);
            }
        }

        public void CopyToClipboard()
        {
            var sb = new StringBuilder();
            foreach (var task in MergeProgressUpdates)
            {
                sb.AppendLine(task);
            }
            Clipboard.SetText(sb.ToString());
        }


        public async Task Ok()
        {
            if (CollaborationDialogAction == CollaborationDialogAction.Import)
            {
                await Import();
            }
            else if (CollaborationDialogAction == CollaborationDialogAction.Merge)
            {
                await Merge();
            }
            else if (CollaborationDialogAction == CollaborationDialogAction.Commit || CollaborationDialogAction == CollaborationDialogAction.Initialize)
            {
                await Commit();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(CollaborationDialogAction), $"Not expected value: {CollaborationDialogAction}");
            }
        }

        public void Report(ProgressStatus status)
        {
            var message = Regex.Replace(status.Message ?? string.Empty, "{PercentCompleted(:.*)?}", "{0$1}");
            var description = Regex.IsMatch(message, "{0(:.*)?}") ?
                string.Format(message, status.PercentCompleted) :
                message;

            if (message.StartsWith("MergeDialog_"))
            {
                description = _localizationService.Get(message);
            }

            OnUIThread(() =>
                {
                    MergeProgressUpdates.Add(description);
                    NotifyOfPropertyChange(nameof(MergeProgressUpdates));
                }
            );

        }

        #endregion // Methods



    }


}
