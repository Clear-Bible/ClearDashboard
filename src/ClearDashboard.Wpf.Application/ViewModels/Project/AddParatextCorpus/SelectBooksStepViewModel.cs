using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpus
{
    public class SelectBooksStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, SelectBooksStepViewModel>
    {

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

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

        private bool _canAdd;
        public bool CanAdd
        {
            get => _canAdd;
            set => Set(ref _canAdd, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public SelectBooksStepViewModel()
        {
            // no-op
        }

        public SelectBooksStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<SelectBooksStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<SelectBooksStepViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {

            DialogMode = dialogMode;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            CanOk = true;
            EnableControls = true;

            CanAdd = false;

        }

        protected async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ParentViewModel.CurrentStepTitle = LocalizationStrings.Get("ParatextCorpusDialog_SelectBooks", Logger);

            var alignment = LocalizationStrings.Get("AddParatextCorpusDialog_Alignment", Logger);

            base.OnActivateAsync(cancellationToken);
        }

        #endregion //Constructor


        #region Methods
        protected override ValidationResult? Validate()
        {
            throw new NotImplementedException();
        }

        public void Ok()
        {
            ParentViewModel?.Ok();
        }

        public async void Add()
        {
            await Add(true);
        }

        public async Task Add(object nothing)
        {
            CanAdd = false;
            _ = await Task.Factory.StartNew(async () =>
            {
 
            }, CancellationToken.None);
        }

        #endregion // Methods



    }
}
