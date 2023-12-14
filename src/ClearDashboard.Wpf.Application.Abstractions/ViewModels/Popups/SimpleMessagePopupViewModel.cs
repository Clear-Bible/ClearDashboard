using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ClearDashboard.Wpf.Application.ViewModels.Popups
{



    public enum SimpleMessagePopupMode
    {
        Add,
        Delete,
        DeleteCorpusNodeConfirmation,
        DeleteProjectConfirmation,
        DeleteCollabProjectSimple,
        DeleteCollabProjectExtended,
        CloseEnhancedViewConfirmation,
        SwitchParatextProjectMessage,
        DeleteParallelLineConfirmation,
    }

    public abstract class SimpleMessagePopupViewModel : DashboardApplicationScreen
    {
        private SimpleMessagePopupMode _simpleMessagePopupMode;


        public static dynamic CreateDialogSettings(string title)
        {

            dynamic settings = new ExpandoObject();
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.Placement = PlacementMode.Absolute;
            settings.HorizontalOffset = SystemParameters.FullPrimaryScreenWidth / 2 - 100;
            settings.VerticalOffset = SystemParameters.FullPrimaryScreenHeight / 2 - 50;
            //settings.WindowStyle = WindowStyle.None;
            //settings.ShowInTaskbar = false;
            //settings.WindowState = WindowState.Normal;
            //settings.ResizeMode = ResizeMode.NoResize;
            settings.Title = title;
            settings.Width = 400;
            settings.Height = 200;
            return settings;

        }

        protected SimpleMessagePopupViewModel()
        {

        }

        protected SimpleMessagePopupViewModel(INavigationService navigationService, ILogger<SimpleMessagePopupViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,
                localizationService)
        {
        }


        public SimpleMessagePopupMode SimpleMessagePopupMode
        {
            get => _simpleMessagePopupMode;
            set
            {
                Set(ref _simpleMessagePopupMode, value);
                NotifyOfPropertyChange(nameof(Title));
            }
        }


        public string? Message => CreateMessage();

        public virtual string? OkLabel => "OK";
        public virtual string? CancelLabel => "Cancel";

        protected virtual string? CreateMessage()
        {
            return "Override CreateMessage!";
        }

        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }
    }
}
