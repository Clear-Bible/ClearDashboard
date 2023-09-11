using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class JiraResultsViewModel : DashboardApplicationScreen
    {
        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        public JiraTicketResponse JiraTicketResponse = new();
        public JiraUser? JiraUser = new();

        #endregion //Public Properties


        #region Observable Properties

        private string _link = string.Empty;
        public string Link
        {
            get => _link;
            set
            {
                _link = value;
                NotifyOfPropertyChange(() => Link);
            }
        }


        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
            }
        }


        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public JiraResultsViewModel()
        {
            // no-op
        }


        public JiraResultsViewModel(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {

        }


        protected override void OnViewAttached(object view, object context)
        {
            if (JiraTicketResponse.Self != null)
            {
                Link = $"https://clearbible.atlassian.net/jira/servicedesk/projects/DUF/queues/custom/19/{JiraTicketResponse.Key}";
            }
            else
            {
                Link = "No Link Found";
            }

            UserName = JiraUser.EmailAddress;
            Password = JiraUser.Password;

            base.OnViewAttached(view, context);
        }



        #endregion //Constructor


        #region Methods


        public void ClickMarkDown()
        {


            if (Link == "")
            {
                return;
            }

            try
            {
                var link = new Uri(Link);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName =link.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }


        public async void Close()
        {
            await TryCloseAsync();
        }


        public void Copy_OnClick(object sender)
        {
            if (sender is Button button)
            {
                switch (button.Name)
                {
                    case "CopyUrl":
                        Clipboard.SetText(Link);
                        break;
                }
            }
            else if (sender is TextBlock block)
            {
                switch (block.Name)
                {
                    case "CopyUsername":
                        Clipboard.SetText(UserName);
                        break;
                    case "CopyPassword":
                        Clipboard.SetText(Password);
                        break;
                }
            }
        }

        #endregion // Methods



    }
}
