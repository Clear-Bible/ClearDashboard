﻿using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Popups;

public class AlignmentPopupViewModel : SimpleMessagePopupViewModel
{

    private TokenDisplayViewModel? _sourceTokenDisplay;
    private TokenDisplayViewModel? _targetTokenDisplay;

    public AlignmentPopupViewModel()
    {
        // required for designer support
    }

    public AlignmentPopupViewModel(INavigationService navigationService, 
        ILogger<AlignmentPopupViewModel> logger,
        DashboardProjectManager? projectManager, 
        IEventAggregator eventAggregator, 
        IMediator mediator,
        ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
        : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope,
            localizationService)
    {
        //no-op
    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    public new string Title 
    {
        get
        {
            switch (SimpleMessagePopupMode)
            {
                case SimpleMessagePopupMode.Add:
                    return "Add Alignment?";
                case SimpleMessagePopupMode.Delete:
                    return "Delete Alignment?";
                default:
                    return string.Empty;
            }
        }
    }

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

    public override string? OkLabel => "Yes";
    public override string? CancelLabel => "No";

    protected override string? CreateMessage()
    {
        switch (SimpleMessagePopupMode)
        {
            case SimpleMessagePopupMode.Add:
            {
                if (TargetTokenDisplay == null && SourceTokenDisplay == null)
                {
                    return "Please set 'TargetTokenDisplay and SourceTokenDisplay";
                }

                return
                    $"Align '{TargetTokenDisplay!.Token.SurfaceText}' with '{SourceTokenDisplay!.Token.SurfaceText}'?";
            }
            case SimpleMessagePopupMode.Delete:
            {
                return "TODO!";
            }
            default:
                return null;
        }
    
      
    }
}