//using Autofac.Core.Lifetime;
//using Autofac;
//using Caliburn.Micro;
//using ClearApplicationFoundation.Exceptions;
//using ClearApplicationFoundation.ViewModels.Infrastructure;
//using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
//using ClearDashboard.DataAccessLayer.Wpf;
//using ClearDashboard.Wpf.Application.Helpers;
//using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using ClearApplicationFoundation.Extensions;
//using ClearBible.Engine.SyntaxTree.Aligner.Translation;
//using ClearDashboard.DataAccessLayer.Models;
//using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
//using ClearDashboard.DAL.Alignment.Translation;
//using SIL.Machine.Corpora;
//using SIL.Machine.Translation;
//using SIL.Machine.Utils;
//using static ClearDashboard.DAL.Alignment.Translation.ITranslationCommandable;

//namespace ClearDashboard.Wpf.Application.ViewModels.Project.SmtModelDialog
//{

//    public class SmtModel
//    {
//        SmtAlgorithm SmtAlgorithm { get; set; }
//    }
//    public class SmtModelDialogViewModel : DashboardApplicationWorkflowShellViewModel
//    {

//        public SmtModelDialogViewModel()
//        {
//            // used by Caliburn Micro for design time 
//            SmtModel = new SmtModel();
//        }

//        public SmtModelDialogViewModel(DashboardProjectManager? projectManager, INavigationService navigationService, ILogger<SmtModelDialogViewModel> logger,
//            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
//        {
//            CanOk = true;
//            DisplayName = LocalizationStrings.Get("ParallelCorpusDialog_TrainSmtModel", Logger);

//            SmtModel = new SmtModel();
//        }

//        private SmtModel? _smtModel;
//        public SmtModel? SmtModel
//        {
//            get => _smtModel;
//            private init => Set(ref _smtModel, value);
//        }

//        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
//        {
//            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("SmtModelDialog", "Order").ToArray();

//            if (views == null || !views.Any())
//            {
//                throw new DependencyRegistrationMissingException(
//                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'SmtModelDialog'.  Please check the dependency registration in your bootstrapper implementation.");
//            }

//            foreach (var view in views)
//            {
//                Steps!.Add(view);
//            }

//            CurrentStep = Steps![0];
//            IsLastWorkflowStep = Steps.Count == 1;

//            EnableControls = true;
//            await ActivateItemAsync(Steps[0], cancellationToken);

//            await base.OnInitializeAsync(cancellationToken);
//        }


//        public bool CanCancel => true /* can always cancel */;
//        public async void Cancel()
//        {
//            await TryCloseAsync(false);
//        }


//        private bool _canOk;
//        public bool CanOk
//        {
//            get => _canOk;
//            set => Set(ref _canOk, value);
//        }

//        public async void Ok()
//        {
//            await TryCloseAsync(true);
//        }

//        public object? ExtraData { get; set; }

//        public async Task TrainSMtModel()
//        {
//            var translationCommandable = new TranslationCommands();

//            //using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
//            //    SmtModelType.FastAlign,
//            //    parallelTextCorpus,
//            //    new DelegateProgress(status =>
//            //        output_.WriteLine($"Training symmetrized Fastalign model: {status.PercentCompleted:P}")),
//            //    SymmetrizationHeuristic.GrowDiagFinalAnd);
//        }
//    }
//}
