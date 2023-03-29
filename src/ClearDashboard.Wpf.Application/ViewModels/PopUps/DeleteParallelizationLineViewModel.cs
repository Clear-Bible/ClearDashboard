using ClearDashboard.Wpf.Application.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.ObjectModel;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class DeleteParallelizationLineViewModel : DashboardApplicationScreen
    {
        private readonly ILogger<SlackMessageViewModel> _logger;

        public DeleteParallelizationLineViewModel()
        {
            // no-op
        }

        public DeleteParallelizationLineViewModel(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
        }


        private ObservableCollection<AlignmentSetId> _alignmentSetIds;
        public ObservableCollection<AlignmentSetId> AlignmentSetIds
        {
            get => _alignmentSetIds;
            set
            {
                _alignmentSetIds = value;
                NotifyOfPropertyChange(() => AlignmentSetIds);
            }
        }


        private ObservableCollection<TranslationSetId> _translationSetIds;
        public ObservableCollection<TranslationSetId> TranslationSetIds
        {
            get => _translationSetIds;
            set
            {
                _translationSetIds = value;
                NotifyOfPropertyChange(() => TranslationSetIds);
            }
        }
    }
}
