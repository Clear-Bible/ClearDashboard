using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using System.Threading;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpus
{
    public class ParatextCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParatextCorpusDialogViewModel
    {
        internal class TaskNames
        {
            public const string PickCorpus = "PickCorpus";
            public const string PickBooks = "PickBooks";
        }

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }


        #endregion //Observable Properties


        #region Constructor

        public ParatextCorpusDialogViewModel()
        {
            // no-op
        }

        public ParatextCorpusDialogViewModel(DialogMode dialogMode,
            DashboardProjectManager? projectManager,
            INavigationService navigationService,
            ILogger<ParatextCorpusDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;

            // TODO
            DisplayName = LocalizationStrings.Get("ParatextCorpusDialog_ParatextCorpus", Logger!);

            DialogMode = dialogMode;
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            var parameters = new List<Autofac.Core.Parameter> { new NamedParameter("dialogMode", DialogMode) };
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("ParallelCorpusDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'ParallelCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = Steps![0];
            IsLastWorkflowStep = Steps.Count == 1;

            EnableControls = true;
            await ActivateItemAsync(Steps[0], cancellationToken);

            await base.OnInitializeAsync(cancellationToken);

        }


        #endregion //Constructor


        #region Methods

        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        #endregion // Methods






    }

    
}
