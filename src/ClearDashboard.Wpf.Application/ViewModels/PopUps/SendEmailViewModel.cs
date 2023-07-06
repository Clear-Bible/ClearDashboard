using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class SendEmailViewModel : DashboardApplicationScreen
    {
        #region Member Variables
        
        private readonly DashboardProjectManager? _projectManager;
        private readonly HttpClientServices _httpClientServices;


        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private List<GitLabGroup> _groups;
        public List<GitLabGroup> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                NotifyOfPropertyChange(() => Groups);
            }
        }

        private GitLabGroup? _selectedGroup;
        public GitLabGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                NotifyOfPropertyChange(() => SelectedGroup);
                CheckEntryFields();
            }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value.Trim();
                NotifyOfPropertyChange(() => FirstName);
                CheckEntryFields();
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value.Trim();
                NotifyOfPropertyChange(() => LastName);
                CheckEntryFields();
            }
        }

        private bool _showCheckUserButton;
        public bool ShowCheckUserButton
        {
            get => _showCheckUserButton;
            set
            {
                _showCheckUserButton = value;
                NotifyOfPropertyChange(() => ShowCheckUserButton);
            }
        }


        #endregion //Observable Properties


        #region Constructor

        public SendEmailViewModel()
        {
            // no-op used by caliburn micro XAML
        }

       
        public SendEmailViewModel(INavigationService navigationService, 
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager, 
            IEventAggregator eventAggregator, 
            IMediator mediator, 
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService,
            HttpClientServices httpClientServices,
            CollaborationManager collaborationManager,
            CollaborationConfiguration collaborationConfiguration)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _projectManager = projectManager;
            _httpClientServices = httpClientServices;
        }

        protected override async void OnViewLoaded(object view)
        {
            if (_projectManager.CurrentUser is not null)
            {
                FirstName = _projectManager.CurrentUser.FirstName ?? string.Empty;
                LastName = _projectManager.CurrentUser.LastName ?? string.Empty;
            }

            base.OnViewLoaded(view);

            if (InternetAvailability.IsInternetAvailable())
            {
                Groups = await _httpClientServices.GetAllGroups();
            }
        }

        #endregion //Constructor


        #region Methods


        private void CheckEntryFields()
        {
            if (FirstName == string.Empty || LastName == string.Empty || SelectedGroup is null)
            {
                ShowCheckUserButton = false;
                return;
            }
            
            ShowCheckUserButton = true;
        }

        public async void DraftEmail()
        {
            var toAddress = "dashbaord@clear.bible";

            var subject = "In-App Registration Message";

            var firstName = FirstName;
            var lastName = LastName;

            var body = $"Hello, my name is {firstName} {lastName} from {SelectedGroup?.CombinedStrings.Trim()}.  I'd like to use ClearDashboard but I don't have a license.  Thank you for your help!";

            string message = string.Format("mailto:{0}?Subject={1}&Body={2}", toAddress, subject, body);

            try
            {
                Process.Start(new ProcessStartInfo(message) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void GroupSelected()
        {
            // for caliburn
        }

        #endregion // Methods

    }
}
