using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using Autofac;
using MediatR;

namespace ClearDashboard.Wpf.Application.ViewModels
{
    public class DashboardViewModel : PaneViewModel, IHandle<LogActivityMessage>, IHandle<CorpusAddedMessage>,
        IHandle<CorpusDeletedMessage>, IHandle<ParallelCorpusAddedMessage>, IHandle<ParallelCorpusDeletedMessage>
    {
        #region Member Variables

        private bool _firstLoad;

        private int _currentMessage = 0;

        #endregion //Member Variables

        #region Public Properties

        public string ContentID { get; set; }
        public DashboardProject DashboardProject { get; set; }

        #endregion //Public Properties

        #region Observable Properties

        private ObservableCollection<string> _messages = new();
        public ObservableCollection<string> Messages

        {
            get => _messages;
            set
            {
                _messages = value;
                NotifyOfPropertyChange(() => Messages);
            }
        }


        #endregion //Observable Properties

        #region commands
        public ICommand ClearLogCommand { get; set; }

        #endregion



        #region Constructor

        public DashboardViewModel()
        {
            Initialize();
        }


        public DashboardViewModel(INavigationService navigationService, ILogger<DashboardViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope lifetimeScope)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            Initialize();

            ClearLogCommand = new RelayCommand(ClearLog);
        }



        private void Initialize()
        {

            this.Title = "📐 DASHBOARD";
            this.ContentId = "DASHBOARD";

            if (!_firstLoad)
            {
                Logger.LogInformation("Not the first load");
            }
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewReady(object view)
        {
            Console.WriteLine();
            base.OnViewReady(view);
        }

        protected override void OnViewLoaded(object view)
        {
            Console.WriteLine();
            base.OnViewLoaded(view);
        }

        #endregion //Constructor

        #region Methods

        public Task HandleAsync(LogActivityMessage message, CancellationToken cancellationToken)
        {
            string shortMessage = message.message.Replace("ClearDashboard.Wpf.ViewModels.", "");

            Messages.Insert(0, $"{_currentMessage} - ({DateTime.Now.ToString("t")}) {shortMessage}");
            _currentMessage++;
            return Task.CompletedTask;
        }

        private void ClearLog(object obj)
        {
            Messages.Clear();
        }

        public Task HandleAsync(CorpusAddedMessage message, CancellationToken cancellationToken)
        {
            Messages.Insert(0, $"{_currentMessage} - ({DateTime.Now.ToString("t")}) CorpusAdded: {message.paratextId}");
            _currentMessage++;
            return Task.CompletedTask;
        }

        public Task HandleAsync(CorpusDeletedMessage message, CancellationToken cancellationToken)
        {
            Messages.Insert(0, $"{_currentMessage} - ({DateTime.Now.ToString("t")}) CorpusDeleted: {message.paratextId}");
            _currentMessage++;
            return Task.CompletedTask;
        }

        public Task HandleAsync(ParallelCorpusAddedMessage message, CancellationToken cancellationToken)
        {
            Messages.Insert(0, $"     --> target: {message.targetParatextId}");
            Messages.Insert(0, $"     --> source: {message.sourceParatextId}");
            Messages.Insert(0, $"{_currentMessage} - ({DateTime.Now.ToString("t")}) ConnectionAdded: ({message.connectorGuid})");

            _currentMessage++;
            return Task.CompletedTask;
        }

        public Task HandleAsync(ParallelCorpusDeletedMessage message, CancellationToken cancellationToken)
        {
            Messages.Insert(0, $"     --> target: {message.targetParatextId}");
            Messages.Insert(0, $"     --> source: {message.sourceParatextId}");
            Messages.Insert(0, $"{_currentMessage} - ({DateTime.Now.ToString("t")}) ConnectionDeleted: ({message.connectorGuid})");

            _currentMessage++;
            return Task.CompletedTask;
        }
        #endregion // Methods
    }
}
