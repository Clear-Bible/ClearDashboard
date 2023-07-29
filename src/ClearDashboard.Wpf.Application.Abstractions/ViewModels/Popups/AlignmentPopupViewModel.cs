using System;
using System.CodeDom;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Popups;

public static class TokenExtensions
{
    public static string GetDisplayText(this Token token)
    {
        switch (token.GetType().Name)
        {
            case "Token":
                return token.SurfaceText;

            case "CompositeToken":
                var compositeToken = (CompositeToken)token;
                var orderedTokens = compositeToken.Tokens.OrderBy(t => t.Position).ToArray();
                var builder = new StringBuilder(orderedTokens.First().SurfaceText);
                foreach (var tokenMember in orderedTokens.Skip(1))
                {
                    builder.Append($" {tokenMember.SurfaceText}");
                }
                return builder.ToString();
            default:
                throw new NotImplementedException("Invalid switch case!");

        }
    }
}

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
                    return LocalizationService!["EnhancedView_AddAlignment"];
                case SimpleMessagePopupMode.Delete:
                    return LocalizationService!["EnhancedView_DeleteAlignment"]; ;
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
                    TargetTokenDisplay!.AlignmentToken.GetDisplayText(), SourceTokenDisplay!.AlignmentToken.GetDisplayText());
               
            }
            case SimpleMessagePopupMode.Delete:
            {
                if (TargetTokenDisplay != null)
                {
                    var alignments = TargetTokenDisplay.VerseDisplay.AlignmentManager!.Alignments!.FindAlignmentsByTokenId(TargetTokenDisplay.AlignmentToken.TokenId.Id).Where(a => a != null);
                    if (alignments.Count() > 1)
                    {
                        SecondaryMessage =
                            string.Format(LocalizationService!["EnhancedView_DeleteAllAlignmentsTemplate"], TargetTokenDisplay!.AlignmentToken.GetDisplayText(), OkLabel);
                        return LocalizationService!["EnhancedView_DeleteAllAlignments"];
                    }
                }

                return string.Format(LocalizationService!["EnhancedView_DeleteAlignmentTemplate"],
                    TargetTokenDisplay!.AlignmentToken.GetDisplayText());

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
            //_debounceTimer.DebounceAsync(10, async () => await UpdateAlignmentStatuses(approvalType));

        }
    }
}