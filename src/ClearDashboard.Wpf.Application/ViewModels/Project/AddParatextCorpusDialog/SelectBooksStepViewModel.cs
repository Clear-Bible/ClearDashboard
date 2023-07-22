using System.Linq;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;

public class SelectBooksStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, SelectBooksStepViewModel>
{
    
    public SelectedBookManager SelectedBookManager { get; private set; }

    #region Public Properties

    #endregion //Public Properties


    #region Observable Properties

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }
    public Visibility NextVisibility { get; } = Visibility.Collapsed;
    public Visibility OkVisibility { get; } = Visibility.Visible;

    private bool _canOk;
    public bool CanOk
    {
        get => _canOk;
        set => Set(ref _canOk, value);
    }

    private bool _canAdd;
    public bool CanAdd
    {
        get => _canAdd;
        set => Set(ref _canAdd, value);
    }

    private bool _continueEnabled;
    public bool ContinueEnabled
    {
        get => _continueEnabled;
        set => Set(ref _continueEnabled, value);
    }

    #endregion //Observable Properties


    #region Constructor

    public SelectBooksStepViewModel()
    {
        // no-op
    }

    public SelectBooksStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager, bool selectBooksStepNextVisible, SelectedBookManager selectedBookManager,
        INavigationService navigationService, ILogger<SelectBooksStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<SelectBooksStepViewModel> validator, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
      
        SelectedBookManager = selectedBookManager;
        if (selectBooksStepNextVisible)
        {
            NextVisibility = Visibility.Visible;
            OkVisibility = Visibility.Collapsed;
        }
        else
        {
            NextVisibility = Visibility.Collapsed;
            OkVisibility = Visibility.Visible;
        }
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        CanOk = true;
        EnableControls = true;

        CanAdd = false;

        ContinueEnabled = false;

    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await SelectedBookManager.InitializeBooks(ParentViewModel.UsfmErrors, ParentViewModel.SelectedProject.Id, true, new CancellationToken());
        ContinueEnabled = SelectedBookManager.SelectedBooks.Any();
        await base.OnActivateAsync(cancellationToken);
    }

    #endregion //Constructor


    #region Methods
    protected override ValidationResult? Validate()
    {
        return null;
    }

    public void Ok()
    {
        ParentViewModel?.BookIds?.AddRange(SelectedBookManager.SelectedAndEnabledBookAbbreviations);
        ParentViewModel?.Ok();
    }

    public async void Next()
    {
        ParentViewModel?.BookIds?.AddRange(SelectedBookManager.SelectedAndEnabledBookAbbreviations);
        await MoveForwards();
    }
    public void Back()
    {
        MoveBackwards();
    }

    // ReSharper disable once UnusedMember.Global
    public void UnselectAllBooks()
    {
        SelectedBookManager.UnselectAllBooks();
    }

    // ReSharper disable once UnusedMember.Global
    public void SelectAllBooks()
    {
        SelectedBookManager.SelectAllBooks();
    }

    // ReSharper disable once UnusedMember.Global
    public void SelectNewTestamentBooks()
    {
        SelectedBookManager.SelectNewTestamentBooks();
    }

    // ReSharper disable once UnusedMember.Global
    public void SelectOldTestamentBooks()
    {
       SelectedBookManager.SelectOldTestamentBooks();
    }

    #endregion // Methods



}