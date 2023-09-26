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
using System.ComponentModel;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;

public class SelectBooksStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, SelectBooksStepViewModel>
{
    private readonly ILocalizationService _localizationService;
    public readonly bool IsUpdateCorpusDialog;
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

    private string _okButtonText;
    public string OkButtonText
    {
        get
        {
            return _okButtonText;
        }
        set
        {
            _okButtonText = value;
            NotifyOfPropertyChange(() => OkButtonText);
        }
    }

    private string _selectBooksLabelText;
    public string SelectBooksLabelText
    {
        get { return _selectBooksLabelText; }
        set
        {
            _selectBooksLabelText = value;
            NotifyOfPropertyChange(() => SelectBooksLabelText);
        }
    }

    #endregion //Observable Properties


    #region Constructor

    public SelectBooksStepViewModel()
    {
        // no-op
    }

    public SelectBooksStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager, bool isUpdateCorpusDialog, SelectedBookManager selectedBookManager,
        INavigationService navigationService, ILogger<SelectBooksStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<SelectBooksStepViewModel> validator, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        _localizationService = localizationService;
        IsUpdateCorpusDialog = isUpdateCorpusDialog;
        SelectedBookManager = selectedBookManager;

        if (IsUpdateCorpusDialog)
        {
            SelectBooksLabelText = _localizationService.Get("UpdateParatextCorpusDialog_SelectBooks");
            OkButtonText = _localizationService.Get("UpdateParatextCorpusDialog_Add");
        }
        else
        {
            SelectBooksLabelText = _localizationService.Get("AddParatextCorpusDialog_SelectedBooks");
            OkButtonText = _localizationService.Get("Ok");
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
        SelectedBookManager.PropertyChanged += OnSelectedBookManagerPropertyChanged;
        await SelectedBookManager.InitializeBooks(ParentViewModel.UsfmErrors, ParentViewModel.SelectedProject.Id, true, new CancellationToken());
        ContinueEnabled = SelectedBookManager.SelectedBooks.Any(book => book.IsSelected);
        await base.OnActivateAsync(cancellationToken);
    }

    protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        SelectedBookManager.PropertyChanged -= OnSelectedBookManagerPropertyChanged;
        return base.OnDeactivateAsync(close, cancellationToken);
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

    public async void Back()
    {
       await MoveBackwards();
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

    private void OnSelectedBookManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var somethingSelected = false;
        var newBooksSelected = false;
        var oldBooksSelected = false;

        foreach (var book in SelectedBookManager.SelectedBooks)
        {
            if (book.IsEnabled && book.IsSelected && !somethingSelected)
            {
                somethingSelected = true;
            }

            if (!book.IsImported && book.IsEnabled && book.IsSelected)
            {
                newBooksSelected = true;
            }

            if (book.IsImported && book.IsEnabled && book.IsSelected)
            {
                oldBooksSelected = true;
            }

            if (somethingSelected && newBooksSelected && oldBooksSelected)
            {
                break;
            }
        }

        if (IsUpdateCorpusDialog)
        {
            OkButtonText = newBooksSelected
                ? _localizationService.Get("UpdateParatextCorpusDialog_UpdateAndAdd")
                : _localizationService.Get("Update");

            if (newBooksSelected && !oldBooksSelected)
            {
                OkButtonText = _localizationService.Get("UpdateParatextCorpusDialog_Add");
            }
        }
        
        ContinueEnabled = somethingSelected;
    }

    #endregion // Methods



}