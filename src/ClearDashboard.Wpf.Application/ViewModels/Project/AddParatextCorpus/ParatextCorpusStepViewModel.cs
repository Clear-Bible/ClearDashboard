using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpus
{
    public class ParatextCorpusStepViewModel: DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, ParatextCorpusStepViewModel>
    {
        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        private DialogMode _dialogMode;
        public DialogMode DialogMode
        {
            get => _dialogMode;
            set => Set(ref _dialogMode, value);
        }

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }

        #endregion //Public Properties


        #region Observable Properties




        #endregion //Observable Properties


        #region Constructor

        public ParatextCorpusStepViewModel()
        {
            // no-op
        }

        public ParatextCorpusStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ParatextCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ParatextCorpusStepViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            DialogMode = dialogMode;
            CanMoveForwards = false;
            CanMoveBackwards = false;
            EnableControls = true;
            CanOk = false;
            CanCreate = false;
        }

        #endregion //Constructor


        #region Methods

        protected override ValidationResult? Validate()
        {
            throw new NotImplementedException();
        }

        #endregion // Methods



    }
}
