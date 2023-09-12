using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class JiraClient : DashboardApplicationScreen
    {
        private readonly ILogger<SlackMessageViewModel> _logger;

        #region Member Variables

        private const string ProjectKey = "DUF";
        private string _jsonTemplate = "{\"fields\": {\"project\": {\"key\": \"[[PROJECT]]\"},\"summary\": \"[[SUMMARY]]\",\"issuetype\": {\"name\": \"Task\"},\"reporter\": {\"accountId\": \"[[ACCOUNTID]]\",\"emailAddress\": \"[[EMAIL]]\",\"displayName\": \"[[EMAIL]]\"},\"description\": [[TEXTHERE]],\"labels\": [\"[[LABEL]]\"]}}";

        private readonly string _secretsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets");
        private const string SecretsFileName = "jiraUser.json";
        private string _secretsFilePath;

        #endregion //Member Variables


        #region Public Properties

        public enum JiraTicketLabel
        {
            LostData = 1,
            CannotCompleteTask = 2,
            DifficultToCompleteTask = 3,
            WantToDo = 4
        }

        #endregion //Public Properties


        #region Constructor

        public JiraClient(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
        DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
        ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;

            _secretsFilePath = Path.Combine(_secretsFolderPath, SecretsFileName);
        }

        #endregion //Constructor


        #region Methods



        /// <summary>
        /// Creates a task ticket in Jira
        /// </summary>
        /// <exception cref="Exception"></exception>
        public async Task<JiraTicketResponse?> CreateTaskTicket(string jiraTitle, string summaryDetail,
            JiraUser? jiraUser, JiraTicketLabel ticketLabel)
        {
            Uri jiraUri = new Uri(Settings.Default.JiraBaseUrl);
            string userName = Settings.Default.JiraUser;
            string apiKey = Settings.Default.JiraToken;


            var authData = Encoding.UTF8.GetBytes($"{userName}:{apiKey}");
            var basicAuthentication = Convert.ToBase64String(authData);

            HttpClient client = new HttpClient();
            client.BaseAddress = jiraUri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);

            var json = _jsonTemplate;
            json = json.Replace("[[PROJECT]]", ProjectKey);
            json = json.Replace("[[SUMMARY]]", jiraTitle);
            json = json.Replace("[[ACCOUNTID]]", jiraUser.AccountId);
            json = json.Replace("[[EMAIL]]", jiraUser.EmailAddress);
            json = json.Replace("[[TEXTHERE]]", summaryDetail);
            json = json.Replace("[[LABEL]]", ticketLabel.ToString());

            //Debug.WriteLine(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await client.PostAsync("/rest/api/3/issue", content);
            if (result.StatusCode == System.Net.HttpStatusCode.Created)
                return await result.Content.ReadFromJsonAsync<JiraTicketResponse>();

            _logger.LogError($"Error creating Jira ticket: {result.StatusCode} - {result.ReasonPhrase}");

            return null;
        }

        /// <summary>
        /// Gets all users from Jira and filters them down to only active users with email addresses
        /// </summary>
        /// <exception cref="Exception"></exception>
        public async Task<List<JiraUser>> GetAllUsers()
        {
            Uri jiraUri = new Uri(Settings.Default.JiraBaseUrl);
            string userName = Settings.Default.JiraUser;
            string apiKey = Settings.Default.JiraToken;


            var authData = Encoding.UTF8.GetBytes($"{userName}:{apiKey}");
            var basicAuthentication = Convert.ToBase64String(authData);

            HttpClient client = new HttpClient();
            client.BaseAddress = jiraUri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);
            var result = await client.GetAsync($"/rest/api/3/users/search");
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var users = await result.Content.ReadFromJsonAsync<List<JiraUser>>();
                if (users is not null)
                {
                    users = users.FindAll(x => x.Active);
                    users = users.FindAll(x => x.EmailAddress != string.Empty);  // remove users without email address
                    users = users.FindAll(x => !x.EmailAddress.Contains("atlassian-demo.invalid")); // remove demo users                    
                }

                //foreach (var user in users)
                //{
                //    Debug.WriteLine(user.DisplayName + " " + user.EmailAddress);
                //}

                return users ?? new List<JiraUser>();
            }


            _logger.LogError($"Error getting all Jira Users: {result.StatusCode} - {result.ReasonPhrase}");
            throw new Exception(await result.Content.ReadAsStringAsync());
        }

        public async Task<JiraUser?> GetUserByEmail(List<JiraUser> jiraUsersList, DashboardUser dashboardUser)
        {
            var user = jiraUsersList.FirstOrDefault(x => x.EmailAddress == dashboardUser.Email);

            if (user != null)
                return user;


            // create a new user in Jira
            var password = GenerateRandomPassword.RandomPassword();

            var createdUser = await CreateJiraUser(dashboardUser.Email, dashboardUser.FullName!, password);
            if (createdUser.EmailAddress != "dirk.kaiser@clear.bible")
            {
                createdUser.Password = password;
                await SaveJiraUser(createdUser);
            }

            return createdUser;
        }


        /// <summary>
        /// Create a new Jira User using the email address from the dashboard user
        /// </summary>
        /// <exception cref="Exception"></exception>
        private async Task<JiraUser?> CreateJiraUser(string? dashboardUserEmail, string fullName, string password)
        {
            Uri jiraUri = new Uri(Settings.Default.JiraBaseUrl);
            string userName = Settings.Default.JiraUser;
            string apiKey = Settings.Default.JiraToken;


            var authData = Encoding.UTF8.GetBytes($"{userName}:{apiKey}");
            var basicAuthentication = Convert.ToBase64String(authData);

            HttpClient client = new HttpClient();
            client.BaseAddress = jiraUri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);

            var json = "{\"name\": \"[[NAME]]\",\"password\": \"[[PASSWORD]]\",\"emailAddress\": \"[[EMAIL]]\",\"displayName\": \"[[DISPLAYNAME]]\"}";
            json = json.Replace("[[NAME]]", dashboardUserEmail);
            json = json.Replace("[[EMAIL]]", dashboardUserEmail);
            json = json.Replace("[[PASSWORD]]", password);
            json = json.Replace("[[DISPLAYNAME]]", fullName);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await client.PostAsync("/rest/api/3/user", content);
            if (result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return await result.Content.ReadFromJsonAsync<JiraUser>();
            }

            _logger.LogError($"Error CreateJiraUser: {result.StatusCode} - {result.ReasonPhrase}");
            return new JiraUser { AccountId = "5fff143cf7ea2a0107ff9f87", DisplayName = "dirk.kaiser@clear.bible", EmailAddress = "dirk.kaiser@clear.bible" };
        }


        public async Task<JiraUser?> LoadJiraUser()
        {
            JiraUser? jiraUser = new();
            if (File.Exists(_secretsFilePath))
            {
                //read the file and convert to JiraUser
                var json = await File.ReadAllTextAsync(_secretsFilePath);

                try
                {
                    jiraUser = JsonSerializer.Deserialize<JiraUser>(json);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error loading JiraUser: {e.Message}");
                }
            }

            return jiraUser;
        }

        public async Task SaveJiraUser(JiraUser? jiraUser)
        {
            try
            {
                //read the file and convert to JiraUser
                var json = JsonSerializer.Serialize(jiraUser);
                await File.WriteAllTextAsync(_secretsFilePath, json);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving JiraUser: {e.Message}");
            }
        }


        #endregion // Methods
    }
}
