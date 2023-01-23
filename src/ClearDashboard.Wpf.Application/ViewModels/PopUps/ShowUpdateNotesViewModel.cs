using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class ShowUpdateNotesViewModel: DashboardApplicationScreen
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<ShowUpdateNotesViewModel> _logger;

        private ObservableCollection<UpdateFormat> _updates = new();
        public ObservableCollection<UpdateFormat> Updates
        {
            get => _updates;
            set
            {
                _updates = value;

                foreach (var update in _updates)
                {
                    if (update.KnownIssues.Count > 0)
                    {
                        KnownIssues = new ObservableCollection<string>(update.KnownIssues);
                        break;
                    }
                }


                NotifyOfPropertyChange(() => Updates);
            }
        }

        private ObservableCollection<string> _knownIssues;
        public ObservableCollection<string> KnownIssues
        {
            get => _knownIssues;
            set
            {
                _knownIssues = value;
                NotifyOfPropertyChange(() => KnownIssues);
            }
        }


        public ShowUpdateNotesViewModel()
        {
            //no-op
        }

        public ShowUpdateNotesViewModel(INavigationService navigationService, ILogger<ShowUpdateNotesViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _navigationService = navigationService;
            _logger = logger;
            //ReleaseNotes = releaseNotes;
        }

        public void Close()
        {
            TryCloseAsync();
        }
    }
}
