﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Aqua.Module.Services;
using FluentValidation.Results;
using FluentValidation;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaAddVersionOrListAssessmentsStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IAquaDialogViewModel, AquaAddVersionOrListAssessmentsStepViewModel>
{

    public class AquaItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public AquaAddVersionOrListAssessmentsStepViewModel()
    {
        OkCommand = new RelayCommand(Ok);
    }
    public AquaAddVersionOrListAssessmentsStepViewModel(
        string aquaId,
        IAquaManager aquaManager,
        IEnhancedViewManager enhancedViewManager,

        DialogMode dialogMode,
        DashboardProjectManager projectManager,
        INavigationService navigationService,
        ILogger<AquaAddVersionOrListAssessmentsStepViewModel> logger,
        IEventAggregator eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope,
        IValidator<AquaAddVersionOrListAssessmentsStepViewModel> validator,
        ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        OkCommand = new RelayCommand(Ok);

        BodyTitle = "Add Version Body Title";
        BodyText = "Add Version Body Text";

    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.StatusBarVisibility = Visibility.Hidden;
        Items.Add(new AquaItem { Id = 1, Name = "Aqua Item 1" });
        Items.Add(new AquaItem { Id = 2, Name = "Aqua Item 2" });
        Items.Add(new AquaItem { Id = 3, Name = "Aqua Item 3" });
        Items.Add(new AquaItem { Id = 4, Name = "Aqua Item 4" });
        Items.Add(new AquaItem { Id = 5, Name = "Aqua Item 5" });
        return base.OnInitializeAsync(cancellationToken);


    }
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return base.OnActivateAsync(cancellationToken);
    }


    public ObservableCollection<AquaItem> Items { get; set; } = new ObservableCollection<AquaItem>();

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
        set => Set(ref bodyText_, value);
    }

    private string? validatedText_;
    public string? ValidatedText
    {
        get => validatedText_;
        set
        {
            Set(ref validatedText_, value);
            //ValidationResult = Validate();
        }
    }

    private string? unvalidatedText_;
    public string? UnvalidatedText
    {
        get => unvalidatedText_;
        set
        {
            Set(ref unvalidatedText_, value);
            //ValidationResult = Validate();
        }
    }

    private string? numericText_;
    public string? NumericText
    {
        get => numericText_;
        set
        {
            Set(ref numericText_, value);
            //ValidationResult = Validate();
        }
    }

    private string? lengthText_;
    public string? LengthText
    {
        get => lengthText_;
        set
        {
            Set(ref lengthText_, value);
            //ValidationResult = Validate();
        }
    }

    public RelayCommand OkCommand { get; }



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

    public async void AddVersion()
    {
        try
        {
            var processStatus = await ParentViewModel!.AddVersion();

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    //await MoveForwards();
                    break;
                case LongRunningTaskStatus.Failed:
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
            ParentViewModel!.Cancel();
        }
    }

    protected override ValidationResult? Validate()
    {
        return Validator.Validate(this);
    }
}