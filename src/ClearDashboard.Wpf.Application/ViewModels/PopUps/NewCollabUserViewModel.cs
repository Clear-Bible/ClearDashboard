using System;
using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Services;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class NewCollabUserViewModel : DashboardApplicationScreen
    {
        private readonly HttpClientServices _httpClientServices;
        private readonly CollaborationConfiguration _collaborationConfiguration;

        #region Member Variables   

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

        private GitLabGroup _selectedGroup = null;
        public GitLabGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                NotifyOfPropertyChange(() => SelectedGroup);
            }
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                NotifyOfPropertyChange(() => FirstName);
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                NotifyOfPropertyChange(() => LastName);
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                NotifyOfPropertyChange(() => Email);
            }
        }

        private CollaborationConfiguration _collaboration = new();
        public CollaborationConfiguration Collaboration
        {
            get => _collaboration;
            set
            {
                _collaboration = value;
                NotifyOfPropertyChange(() => Collaboration);
            }
        }

        private string _emailCode = "";
        public string EmailCode
        {
            get => _emailCode;
            set
            {
                _emailCode = value;
                NotifyOfPropertyChange(() => EmailCode);
            }
        }



        #endregion //Observable Properties


        #region Constructor

        public NewCollabUserViewModel()
        {
            // no-op used by caliburn micro XAML
        }

       


        public NewCollabUserViewModel(INavigationService navigationService, 
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager, 
            IEventAggregator eventAggregator, 
            IMediator mediator, 
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService,
            HttpClientServices httpClientServices,
            CollaborationConfiguration collaborationConfiguration)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _httpClientServices = httpClientServices;
            _collaborationConfiguration = collaborationConfiguration;
        }

        protected override async void OnViewLoaded(object view)
        {
            if (InternetAvailability.IsInternetAvailable())
            {
                Groups = await _httpClientServices.GetAllGroups();
            }
            base.OnViewLoaded(view);
        }


        //protected override void OnViewReady(object view)
        //{

        //    Console.WriteLine();
        //    base.OnViewReady(view);
        //}


        #endregion //Constructor


        #region Methods

        public void GroupSelected()
        {
            // no-op
        }

        public async void CreateGitLabUser()
        {
            Console.WriteLine();
            var password = GenerateRandomPassword.RandomPassword(16);

            GitLabUser user = await _httpClientServices.CreateNewUser(FirstName, LastName, GetUserName(), password,
                Email, SelectedGroup.Name);
        }

        /// <summary>
        /// checks to see if the user is already on the system
        /// </summary>
        public async void CheckUser()
        {
            var username = GetUserName();
            var isAlreadyOnGitLab = await _httpClientServices.CheckForExistingUser(username);

            // not a user
            if (isAlreadyOnGitLab == false)
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("cleardas@cleardashboard.org", "cleardas@cleardashboard.org"));
                mailMessage.To.Add(new MailboxAddress(FirstName + " " + LastName, Email));
                mailMessage.Subject = "ClearDashboard Email Validation Code";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Hello"
                };

                try
                {
                    using (var smtpClient = new SmtpClient())
                    {
                        //smtpClient.Connect("mail.gmail.com", 587, true);
                        smtpClient.Connect("mail.cleardashboard.org", 587, true);
                        smtpClient.Authenticate("cleardas@cleardashboard.org", "SP$s74g7h5@o_OZiPrU9");
                        smtpClient.Send(mailMessage);
                        smtpClient.Disconnect(true);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private string GetUserName()
        {
            return (FirstName.Trim() + "." + LastName.Trim()).ToLower();
        }


        public void ValidateEmailCode()
        {
            Console.WriteLine();
        }

        #endregion // Methods

    }
}
