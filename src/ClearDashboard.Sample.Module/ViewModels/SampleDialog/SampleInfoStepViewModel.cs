using Autofac;
using Caliburn.Micro;
using ClearDashboard.Sample.Module.ViewModels.SampleDialog;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.Framework.Input;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Sample.Module.ViewModels.SampleDialog
{
    public class SampleInfoStepViewModel : DashboardApplicationWorkflowStepViewModel<ISampleDialogViewModel>
    {
        public SampleInfoStepViewModel()
        {
            OkCommand = new RelayCommand(Ok);
        }
        public SampleInfoStepViewModel(
            string aquaId,

            DialogMode dialogMode,
            DashboardProjectManager projectManager,
            INavigationService navigationService,
            ILogger<SampleAddRevisionStepViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            DialogMode = dialogMode;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;

            OkCommand = new RelayCommand(Ok);

            BodyTitle = "Info Body Title";
            BodyText = "Info Body Text";

        }
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            ParentViewModel!.StatusBarVisibility = Visibility.Hidden;
            return base.OnInitializeAsync(cancellationToken);
        }
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }


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
        public RelayCommand OkCommand { get; }



        public void Ok(object obj)
        {
            ParentViewModel!.Ok();
        }

        public void Cancel(object obj)
        {
            ((ISampleDialogViewModel)ParentViewModel!).Cancel();
        }
        public async void MoveForwards(object obj)
        {
            await MoveForwards();
        }
        public async void MoveBackwards(object obj)
        {
            await MoveBackwards();
        }
    }
}