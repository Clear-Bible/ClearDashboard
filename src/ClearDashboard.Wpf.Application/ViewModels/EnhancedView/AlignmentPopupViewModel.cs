using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{

    public enum AlignmentPopupResult
    {
        Yes,
        No
    }

    public enum AlignmentPopupMode
    {
        Add,
        Delete
    }

    public class AlignmentPopupViewModel : DashboardApplicationScreen
    {
        private TokenDisplayViewModel? _sourceTokenDisplay;
        private TokenDisplayViewModel? _targetTokenDisplay;
        
        public AlignmentPopupViewModel()
        {
            
        }

        public AlignmentPopupViewModel(INavigationService navigationService, ILogger<AlignmentPopupViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,
                localizationService)
        {
        }

        public AlignmentPopupResult AlignmentPopupResult { get;  set; }

        public AlignmentPopupMode AlignmentPopupMode { get;  set; }

        public TokenDisplayViewModel? SourceTokenDisplay
        {
            get => _sourceTokenDisplay;
            set
            {
                Set(ref _sourceTokenDisplay, value);
                NotifyOfPropertyChange(nameof(Message));
            }
        }

        public TokenDisplayViewModel? TargetTokenDisplay
        {
            get => _targetTokenDisplay;
            set
            {
                Set(ref _targetTokenDisplay, value); 
                NotifyOfPropertyChange(nameof(Message));
            }
        }

        public string Message => CreateMessage();

        private string CreateMessage()
        {
            if (TargetTokenDisplay == null && SourceTokenDisplay == null)
            {
                return "Message goes here.";
            }
            return $"Align {TargetTokenDisplay.Token.SurfaceText} with {SourceTokenDisplay.Token.SurfaceText}?";
        }

        public async void Yes()
        {
            await TryCloseAsync(true);
        }

        public async void No()
        {
            await TryCloseAsync(false);
        }
    }
}
