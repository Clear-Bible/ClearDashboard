using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;

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
                case SimpleMessagePopupMode.DeleteParallelLineConfirmation:
                    return "Confirmation";
                case SimpleMessagePopupMode.ExistingProjectNameTheSame:
                    return LocalizationService!["Pds_ExistingProjectName"];
                case SimpleMessagePopupMode.StartParatextFirst:
                    return "Start Paratext First";
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
                case SimpleMessagePopupMode.ExistingProjectNameTheSame:
                case SimpleMessagePopupMode.StartParatextFirst:
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
                case SimpleMessagePopupMode.ExistingProjectNameTheSame:
                case SimpleMessagePopupMode.StartParatextFirst:
                    return Visibility.Collapsed;
                default:
                    return Visibility.Visible;
            }
        }
    }

    private string _subHeader = "";
    public string SubHeader
    {
        get => _subHeader;
        set 
        { 
            _subHeader = value; 
            NotifyOfPropertyChange(() => SubHeader);
        }
    }

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
            case SimpleMessagePopupMode.DeleteParallelLineConfirmation:
                return LocalizationService!["Pds_DeleteParallelLine"];
            case SimpleMessagePopupMode.ExistingProjectNameTheSame:
                return LocalizationService!["Pds_ExistingProject"];
            case SimpleMessagePopupMode.StartParatextFirst:
                return LocalizationService!["Pds_StartParatextFirst"];
            default:
                return string.Empty;
        }
    }
}