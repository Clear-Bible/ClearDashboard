using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Exceptions;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaAssessmentStepViewModel : DashboardApplicationWorkflowStepViewModel<IAquaDialogViewModel>
{
    private readonly IAquaManager? aquaManager_;

    public AquaAssessmentStepViewModel()
    {
    }
    public AquaAssessmentStepViewModel(
        IAquaManager aquaManager,

        DialogMode dialogMode,  
        DashboardProjectManager projectManager,
        INavigationService navigationService, 
        ILogger<AquaAssessmentStepViewModel> logger, 
        IEventAggregator eventAggregator,
        IMediator mediator, 
        ILifetimeScope? lifetimeScope,
        ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        aquaManager_ = aquaManager;
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        BodyTitle = LocalizationService!.Get("Aqua_AssessmentStep_BodyTitle");
        BodyText = LocalizationService!.Get("Aqua_AssessmentStep_BodyText"); ;
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        return base.OnInitializeAsync(cancellationToken);
    }
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
        return base.OnActivateAsync(cancellationToken);
    }

    //private string? revisionId_;
    //public string? RevisionId
    //{
    //    get => revisionId_;
    //    set
    //    {
    //        Set(ref revisionId_, value);
    //        //ValidationResult = Validate();
    //    }
    //}

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private string? bodyTitle_;
    public string? BodyTitle
    {
        get => bodyTitle_;
        set
        {
            bodyTitle_ = value;
            NotifyOfPropertyChange(() => BodyTitle);
        }
    }

    private string? bodyText_;

    public string? BodyText
    {
        get => bodyText_;
        set
        {
            bodyText_ = value;
            NotifyOfPropertyChange(() => BodyText);
        }
    }

    private int? id_ = null;
    //public int? Id
    //{
    //    get => id_;
    //    set
    //    {
    //        Set(ref id_, value);
    //        //ValidationResult = Validate();
    //    }
    //}

    private string? name_ = null;
    public string? Name
    {
        get => name_;
        set
        {
            Set(ref name_, value == "" ? null : value);
            //ValidationResult = Validate();
        }
    }

    private bool published_ = false;
    public bool Published
    {
        get => published_;
        set
        {
            Set(ref published_, value);
        }
    }
    public void Ok(object obj)
    {
        ParentViewModel!.Ok();
    }
    public void Cancel(object obj)
    {
        ParentViewModel!.Cancel();
    }
    public async void MoveForwards(object obj)
    {
        await MoveForwards();
    }
    public async void MoveBackwards(object obj)
    {
        await MoveBackwards();
    }
    public async void AddAssessment()
    {
        try
        {
            var processStatus = await ParentViewModel!.RunLongRunningTask(
                "AQuA-Add_Assessment",
                (cancellationToken) => aquaManager_!.AddAssessment(
                    new Assessment(1,null,null,null,null,null,null,null), //fixme
                    cancellationToken),
                (assessment) => {
                    if (assessment == null)
                        throw new InvalidParameterEngineException(name: "assessment", value: "null", message: "AddAssessment was successful but returned null");
                    //Name = revision.name;
                    //Published= revision.published;
                    //id_ = 
                });

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    //await MoveForwards();
                    await MoveBackwards();
                    break;
                case LongRunningTaskStatus.Failed:
                    break;
                case LongRunningTaskStatus.Cancelled:
                    ParentViewModel!.Cancel();
                    break;
                case LongRunningTaskStatus.NotStarted:
                    break;
                case LongRunningTaskStatus.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception)
        {
            //ParentViewModel!.Cancel();
        }
    }
}