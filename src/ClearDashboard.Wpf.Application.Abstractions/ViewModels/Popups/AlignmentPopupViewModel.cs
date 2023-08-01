using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ClearDashboard.DAL.Alignment.Translation;

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
        Verification = AlignmentVerificationStatus.Verified;
        return base.OnInitializeAsync(cancellationToken);
    }

    public new string Title 
    {
        get
        {
            switch (SimpleMessagePopupMode)
            {
                case SimpleMessagePopupMode.Add:
                    return LocalizationService!["EnhancedView_AddAlignment"];
                case SimpleMessagePopupMode.Delete:
                    return LocalizationService!["EnhancedView_DeleteAlignment"]; ;
                default:
                    return string.Empty;
            }
        }
    }

    public string Verification { get; private set; }

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

    public override string? OkLabel => LocalizationService!["Yes"];
    public override string? CancelLabel => LocalizationService!["No"];

    public string? SecondaryMessage { get; set; } = null;
    

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
                

                return string.Format(LocalizationService!["EnhancedView_AddAlignmentTemplate"],
                    TargetTokenDisplay!.GetDisplayText(), SourceTokenDisplay!.GetDisplayText());
               
            }
            case SimpleMessagePopupMode.Delete:
            {
                if (TargetTokenDisplay != null)
                {
                    var alignments = TargetTokenDisplay.VerseDisplay.AlignmentManager!.Alignments!.FindAlignmentsByTokenId(TargetTokenDisplay.AlignmentToken.TokenId.Id).Where(a => a != null);
                    if (alignments.Count() > 1)
                    {
                        SecondaryMessage =
                            string.Format(LocalizationService!["EnhancedView_DeleteAllAlignmentsTemplate"], TargetTokenDisplay!.GetDisplayText(), OkLabel);
                        return LocalizationService!["EnhancedView_DeleteAllAlignments"];
                    }
                }

                return string.Format(LocalizationService!["EnhancedView_DeleteAlignmentTemplate"],
                    TargetTokenDisplay!.GetDisplayText());

                }
            default:
                return null;
        }
    
      
    }

    public void OnAlignmentApprovalChanged(SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
        {
            var approvalType = (item.Tag as string);

            Verification = approvalType.DetermineAlignmentVerificationStatus();
        }
    }
}