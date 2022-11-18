using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class ShowUpdateNotesViewModel: DashboardApplicationScreen
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<ShowUpdateNotesViewModel> _logger;


        //private ObservableCollection<ReleaseNote> _releaseNotes = new();
        //public ObservableCollection<ReleaseNote> ReleaseNotes
        //{
        //    get => _releaseNotes;
        //    set
        //    {
        //        _releaseNotes = value;
        //        NotifyOfPropertyChange(() => ReleaseNotes);
        //    }
        //}

        private ObservableCollection<UpdateFormat> _updates = new();
        public ObservableCollection<UpdateFormat> Updates
        {
            get => _updates;
            set
            {
                _updates = value;
                NotifyOfPropertyChange(() => Updates);
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
    }
}
