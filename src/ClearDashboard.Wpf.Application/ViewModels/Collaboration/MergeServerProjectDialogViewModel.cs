using Autofac;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using ClearApplicationFoundation.Services;

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
        private readonly GitLabHttpClientServices _gitLabHttpClientServices;
        private CancellationTokenSource _cancellationTokenSource;
        private Task? _runningTask;

        private CollaborationConfiguration _userInfo;
        private GitLabUser _gitLabUser;

        private ILocalizationService _localizationService;
        private string DialogTitle => $"{OkAction} Server Project: {ProjectName}";

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


        public string OkAction => CollaborationDialogAction.ToString();

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
                Organization = _userInfo.Group
            };
            await CreateProjectOnServerIfNotCreated();

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

        private async Task CreateProjectOnServerIfNotCreated()
        {
            var projects = await _gitLabHttpClientServices.GetProjectsForUser(_userInfo);
            var project = projects.FirstOrDefault(x => x.Name == $"P_{ProjectId}");

            if (project is null)
            {
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
        }


        private bool PreAction()
        {
            if (ProjectId == Guid.Empty)
            {
                return false;
            }

            if (CheckIfConnectedToParatext() == false)
            {
                return false;
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

            CancelAction = "Close";
            ProgressBarVisibility = Visibility.Hidden;
        }

        private async Task Import()
        {
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
            }
            catch (OperationCanceledException)
            {
                progress.Report(new ProgressStatus(0, "Operation Cancelled"));
                PlaySound.PlaySoundFromResource(SoundType.Error);
            }
            catch (Exception ex)
            {
                progress.Report(new ProgressStatus(0, $"Exception thrown attempting to initialize project database: {ex.Message}"));
                CanOkAction = true;
                PlaySound.PlaySoundFromResource(SoundType.Error);
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
            }
            catch (OperationCanceledException)
            {
                progress.Report(new ProgressStatus(0, "Operation Cancelled"));
                PlaySound.PlaySoundFromResource(SoundType.Error);
            }
            catch (Exception ex)
            {
                progress.Report(new ProgressStatus(0, $"Exception thrown attempting to merge latest project changes: {ex.Message}"));
                PlaySound.PlaySoundFromResource(SoundType.Error);
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
                });

                await _runningTask;
            }
            catch (OperationCanceledException)
            {
                progress.Report(new ProgressStatus(0, "Operation Cancelled"));
                PlaySound.PlaySoundFromResource(SoundType.Error);
            }
            catch (Exception ex)
            {
                if (CollaborationDialogAction == CollaborationDialogAction.Initialize)
                {
                    progress.Report(new ProgressStatus(0, $"Exception thrown attempting to initialize, fetch, stage and commit project changes: {ex.Message}"));
                }
                else
                {
                    progress.Report(new ProgressStatus(0, $"Exception thrown attempting to stage and commit project changes: {ex.Message}"));
                }
                PlaySound.PlaySoundFromResource(SoundType.Error);
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
                await TryCloseAsync(false);
            }
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
