using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class ImportWordAnalysesDialogViewModel : DashboardApplicationScreen
    {
        #region Member Variables   

        private readonly ILogger<ImportWordAnalysesDialogViewModel> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task? _runningTask;

        private ILocalizationService _localizationService;
        private bool _completedTask = false;

		#endregion //Member Variables

		#region Public Properties

		private TokenizedTextCorpusId? _tokenizedTextCorpusId = null;
		public TokenizedTextCorpusId? TokenizedTextCorpusId
		{
			get => _tokenizedTextCorpusId;
			set => _tokenizedTextCorpusId = value;
		}

		#endregion //Public Properties

		#region Observable Properties

		private string _tokenizedTextCorpusDisplayName = string.Empty;
		public string TokenizedTextCorpusDisplayName
		{
			get => _tokenizedTextCorpusDisplayName;
			set => Set(ref _tokenizedTextCorpusDisplayName, value);
		}

		private Visibility? _progressBarVisibility = Visibility.Hidden;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        private bool _canCancelAction = true;
        public bool CanCancelAction
        {
            get => _canCancelAction;
            set => Set(ref _canCancelAction, value);
        }

        private string _cancelAction = string.Empty;
        public string CancelAction
        {
            get => _cancelAction;
            set
            {
                _cancelAction = value;
                NotifyOfPropertyChange(() => CancelAction);
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        private System.Windows.Media.Brush statusMessageColor = System.Windows.Media.Brushes.Black;
        public System.Windows.Media.Brush StatusMessageColor 
        { 
            get => statusMessageColor; 
            set => Set(ref statusMessageColor, value); 
        }

        #endregion //Observable Properties

        #region Constructor

        public ImportWordAnalysesDialogViewModel()
        {
            // no-op used for Caliburn Micro
        }

        public ImportWordAnalysesDialogViewModel(CollaborationManager collaborationManager,
            DashboardProjectManager projectManager,
            INavigationService navigationService,
            ILogger<ImportWordAnalysesDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _localizationService = localizationService;
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger;

            //return base.OnInitializeAsync(cancellationToken);
        }


        protected override async void OnViewLoaded(object view)
        {
            await Ok();
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

        private bool PreAction()
        {
            StatusMessage = string.Empty;

            if (TokenizedTextCorpusId is null)
            {
                return false;
            }

            if (CheckIfConnectedToParatext() == false)
            {
                return false;
            }

            CancelAction = _localizationService["Cancel"];
            ProgressBarVisibility = Visibility.Visible;
            TokenizedTextCorpusDisplayName = TokenizedTextCorpusId?.DisplayName ?? string.Empty;

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
            try
            {
                if (!PreAction())
                {
                    return;
                }

                _runningTask = Task.Run(async () => {

					var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, TokenizedTextCorpusId!);
					await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, _cancellationTokenSource.Token);

				});
                await _runningTask;

                PlaySound.PlaySoundFromResource();

                _completedTask = true;

                _logger.LogInformation($"Import Word Analyses for Tokenized Corpus '{TokenizedTextCorpusId?.DisplayName}' (Id '{TokenizedTextCorpusId?.Id}') completed.");

                StatusMessage = _localizationService["ImportWordAnalyses_ImportComplete"];
                StatusMessageColor = System.Windows.Media.Brushes.Green;
                CancelAction = _localizationService["Done"];
			}
            catch (OperationCanceledException)
            {
                PlaySound.PlaySoundFromResource(SoundType.Disconnected);

				_logger.LogInformation($"Import Word Analyses for Tokenized Corpus '{TokenizedTextCorpusId?.DisplayName}' (Id '{TokenizedTextCorpusId?.Id}') cancelled.");

				StatusMessage = _localizationService["ImportWordAnalyses_ImportCancelled"];
                StatusMessageColor = System.Windows.Media.Brushes.Orange;
                CancelAction = _localizationService["Close"];
			}
            catch (Exception ex)
            {
                PlaySound.PlaySoundFromResource(SoundType.Error);

				_logger.LogError(ex, $"Import Word Analyses for Tokenized Corpus '{TokenizedTextCorpusId?.DisplayName}' (Id '{TokenizedTextCorpusId?.Id}') failed");

				StatusMessage = _localizationService["ImportWordAnalyses_ImportErrored"]; 
                StatusMessageColor = System.Windows.Media.Brushes.Red;
                CancelAction = _localizationService["Close"];
			}
            finally
            {
                // If Import succeeds, don't set CanAction back to true
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

        private async Task CancelRunningTask()
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
            settings.Width = 400;
            settings.Height = 300;
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

		public async Task Ok()
        {
            await Import();
        }

        #endregion // Methods
    }
}
