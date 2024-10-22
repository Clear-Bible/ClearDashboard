﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class SmtModelStepViewModel : DashboardApplicationWorkflowStepViewModel<IParallelCorpusDialogViewModel>
{
    private readonly ILocalizationService _localizationService;

    #region Member Variables   

    #endregion //Member Variables


    #region Public Properties

    #endregion //Public Properties


    #region Observable Properties

    
    private ObservableCollection<SmtAlgorithm> _smtList = new();
    public ObservableCollection<SmtAlgorithm> SmtList
    {
        get => _smtList;
        set
        {
            _smtList = value; 
            NotifyOfPropertyChange(() => SmtList);  
        }
    }


    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }


    private bool _canTrain;
    public bool CanTrain
    {
        get => _canTrain;
        set => Set(ref _canTrain, value);
    }

    private bool _isTrainedSymmetrizedModel;
    public bool IsTrainedSymmetrizedModel
    {
        get => _isTrainedSymmetrizedModel;
        set
        {
            Set(ref _isTrainedSymmetrizedModel, value);
            ParentViewModel!.IsTrainedSymmetrizedModel = value;
        }
    }


    // ReSharper disable once InconsistentNaming
    private bool _SMTsReady;
    // ReSharper disable once InconsistentNaming
    public bool SMTsReady
    {
        get => _SMTsReady;
        set => Set(ref _SMTsReady, value);
    }


    #endregion //Observable Properties


    #region Constructor

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SmtModelStepViewModel()
    {
        // no op
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    public SmtModelStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<SmtModelStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        _localizationService = localizationService;
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanTrain = true;
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.CurrentStepTitle =
            LocalizationService!.Get("ParallelCorpusDialog_TrainSmtModel");

        if (ParentViewModel.UseDefaults)
        {
            await Train(true);
        }
        
        try
        {
            var parallelCorpa = ParentViewModel.TopLevelProjectIds.ParallelCorpusIds.Where(x =>
                x.SourceTokenizedCorpusId!.CorpusId!.Id == ParentViewModel.SourceCorpusNodeViewModel.CorpusId
                && x.TargetTokenizedCorpusId!.CorpusId!.Id == ParentViewModel.TargetCorpusNodeViewModel.CorpusId
            ).ToList();

            List<string> smts = new();
            foreach (var parallelCorpusId in parallelCorpa)
            {
                var alignments =
                    ParentViewModel.TopLevelProjectIds.AlignmentSetIds.Where(x =>
                        x.ParallelCorpusId!.Id == parallelCorpusId.Id);
                foreach (var alignment in alignments)
                {
                    smts.Add(alignment.SmtModel!);
                }
            }

            // create a new list for the SMT enums
            OnUIThread(() =>
            {
                SmtList.Clear();
            });

            var list = Enum.GetNames(typeof(SmtModelType)).ToList();
            foreach (var smt in list)
            {
                if (smts.Contains(smt))
                {
                    OnUIThread(() =>
                    {
                        SmtList.Add(new SmtAlgorithm
                        {
                            SmtName = smt,
                            IsEnabled = false,
                        });
                    });
                }
                else
                {
                    // ReSharper disable once InconsistentNaming
                    var newSMT = new SmtAlgorithm
                    {
                        SmtName = smt,
                        IsEnabled = true,
                    };
                
                    OnUIThread(() =>
                    {
                        SmtList.Add(newSMT);
                    });
                }
            }

            // select next available smt that is enabled
            bool found = false;
            foreach (var smt in SmtList)
            {
                if (smt.IsEnabled)
                {
                    found = true;
                    ParentViewModel.SelectedSmtAlgorithm = smt;
                    break;
                }
            }

            if (found)
            {
                SMTsReady = true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        await base.OnActivateAsync(cancellationToken);
    }

    #endregion //Constructor


    #region Methods

    public async void Train()
    {
        if (ParentViewModel.SelectedSmtAlgorithm is null)
        {
            var msg = _localizationService["ParatextCorpusDialog_SelectSmt"];
            ParentViewModel.Message = msg;

            return;
        }

        ParentViewModel.Message = "";
        await Train(true);
    }


    public async Task Train(object nothing)
    {


        CanTrain = false;
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.TrainSmtModel(IsTrainedSymmetrizedModel);

                switch (processStatus)
                {
                    case LongRunningTaskStatus.Completed:
                        await MoveForwards();
                        break;
                    case LongRunningTaskStatus.Failed:
                    case LongRunningTaskStatus.Cancelled:
                        ParentViewModel.Cancel();
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
        }, CancellationToken.None);
    }

    #endregion // Methods
}