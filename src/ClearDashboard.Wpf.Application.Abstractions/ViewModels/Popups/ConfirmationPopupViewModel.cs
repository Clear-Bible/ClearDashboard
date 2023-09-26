using System;
using System.CodeDom;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Popups;
public class ConfirmationPopupViewModel : SimpleMessagePopupViewModel
{

    public ConfirmationPopupViewModel()
    {
        // required for designer support
    }

    public ConfirmationPopupViewModel(INavigationService navigationService, 
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
                case SimpleMessagePopupMode.CloseEnhancedViewConfirmation:
                    return "Confirmation";
                case SimpleMessagePopupMode.DeleteCorpusNodeConfirmation:
                    return "Confirmation";
                case SimpleMessagePopupMode.DeleteCollabProjectSimple:
                case SimpleMessagePopupMode.DeleteCollabProjectExtended:
                    return LocalizationService!["CollabProjectManagementView_Delete"];
                case SimpleMessagePopupMode.DeleteProjectConfirmation:
                    return LocalizationService!["Delete_Project"];
                case SimpleMessagePopupMode.SwitchParatextProjectMessage:
                    return "Switch Paratext Project";
                default:
                    return string.Empty;
            }
        }
    }

    public override string? OkLabel
    {
        get
        {
            switch (SimpleMessagePopupMode)
            {
                case SimpleMessagePopupMode.SwitchParatextProjectMessage:
                    return LocalizationService!["Ok"];
                default:
                    return LocalizationService!["Yes"];
            }
        }
    }

    public override string? CancelLabel => LocalizationService!["No"];

    public Visibility? CancelVisibility
    {
        get
        {
            switch (SimpleMessagePopupMode)
            {
                case SimpleMessagePopupMode.SwitchParatextProjectMessage:
                    return Visibility.Collapsed;
                default:
                    return Visibility.Visible;
            }
        }
    }

    //public string? SecondaryMessage { get; set; } = null;

    protected override string? CreateMessage()
    {
        switch (SimpleMessagePopupMode)
        {
            case SimpleMessagePopupMode.CloseEnhancedViewConfirmation:
                return LocalizationService!["EnhancedView_ClosingEnhancedView"];
            case SimpleMessagePopupMode.DeleteCorpusNodeConfirmation:
                return LocalizationService!["Pds_DeletingCorpusNode"];
            case SimpleMessagePopupMode.DeleteCollabProjectSimple:
                return LocalizationService!["CollabProjectManagementView_DeleteConfirmSimple"];
            case SimpleMessagePopupMode.DeleteCollabProjectExtended:
                return LocalizationService!["CollabProjectManagementView_DeleteConfirmExtended"];
            case SimpleMessagePopupMode.DeleteProjectConfirmation:
                return LocalizationService!["Delete_Project_Confirmation"]; 
            case SimpleMessagePopupMode.SwitchParatextProjectMessage:
                return LocalizationService!["Pds_SwitchParatextProject"]; 
            default:
                return string.Empty;
        }
    }
}