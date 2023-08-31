using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class JiraClient
    {
        private static string _ProjectKey = "DUF";
        private static string _jsonTemplate = "{\"fields\": {\"project\": {\"key\": \"[[PROJECT]]\"},\"summary\": \"[[SUMMARY]]\",\"issuetype\": {\"name\": \"Task\"},\"reporter\": {\"accountId\": \"[[ACCOUNTID]]\",\"emailAddress\": \"[[EMAIL]]\",\"displayName\": \"[[EMAIL]]\"},\"description\": [[TEXTHERE]],\"labels\": [\"[[LABEL]]\"]}}";


        public enum JiraTicketLabel
        {
            LostData = 1,
            CannotCompleteTask = 2,
            DifficultToCompleteTask = 3,
            WantToDo = 4
        }


        /// <summary>
        /// Creates a task ticket in Jira
        /// </summary>
        /// <param name="jiraTitle"></param>
        /// <param name="summaryDetail"></param>
        /// <param name="severityText"></param>
        /// <param name="jiraUser"></param>
        /// <param name="summary"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<JiraTicketResponse> CreateTaskTicket(string jiraTitle, string summaryDetail,
            JiraUser jiraUser, JiraTicketLabel ticketLabel)
        {
            Uri jiraUri = new Uri(Settings.Default.JiraBaseUrl);
            string userName = Settings.Default.JiraUser;
            string apiKey = Settings.Default.JiraToken;


            var authData = System.Text.Encoding.UTF8.GetBytes($"{userName}:{apiKey}");
            var basicAuthentication = Convert.ToBase64String(authData);

            HttpClient client = new HttpClient();
            client.BaseAddress = jiraUri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);

            var json = _jsonTemplate;
            json = json.Replace("[[PROJECT]]", _ProjectKey);
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

            return null;
        }

        /// <summary>
        /// Gets all users from Jira and filters them down to only active users with email addresses
        /// </summary>
        /// <param name="jiraUri"></param>
        /// <param name="userName"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<JiraUser>> GetAllUsers()
        {
            Uri jiraUri = new Uri(Settings.Default.JiraBaseUrl);
            string userName = Settings.Default.JiraUser;
            string apiKey = Settings.Default.JiraToken;


            var authData = System.Text.Encoding.UTF8.GetBytes($"{userName}:{apiKey}");
            var basicAuthentication = Convert.ToBase64String(authData);

            HttpClient client = new HttpClient();
            client.BaseAddress = jiraUri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);
            var result = await client.GetAsync($"/rest/api/3/users/search");
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var users = await result.Content.ReadFromJsonAsync<List<JiraUser>>();
                users = users.FindAll(x => x.Active == true);
                users = users.FindAll(x => x.EmailAddress != string.Empty);  // remove users without email address
                users = users.FindAll(x => !x.EmailAddress.Contains("atlassian-demo.invalid")); // remove demo users

                //foreach (var user in users)
                //{
                //    Debug.WriteLine(user.DisplayName + " " + user.EmailAddress);
                //}

                return users;
            }

            throw new Exception(await result.Content.ReadAsStringAsync());
        }

        public static async Task<JiraUser> GetUserByEmail(List<JiraUser> jiraUsersList, DashboardUser dashboardUser)
        {
            var user = jiraUsersList.FirstOrDefault(x => x.EmailAddress == dashboardUser.Email);

            if (user != null)
                return user;


            // create a new user in Jira
            var password = GenerateRandomPassword.RandomPassword();

            var createdUser = await CreateJiraUser(dashboardUser.Email, dashboardUser.FullName, password);
            if (createdUser != null)
            {
                createdUser.Password = password;
                return createdUser;
            }

            return null;
        }


        /// <summary>
        /// Create a new Jira User using the email address from the dashboard user
        /// </summary>
        /// <param name="dashboardUserEmail"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task<JiraUser> CreateJiraUser(string? dashboardUserEmail, string fullName, string password)
        {
            Uri jiraUri = new Uri(Settings.Default.JiraBaseUrl);
            string userName = Settings.Default.JiraUser;
            string apiKey = Settings.Default.JiraToken;


            var authData = System.Text.Encoding.UTF8.GetBytes($"{userName}:{apiKey}");
            var basicAuthentication = Convert.ToBase64String(authData);

            HttpClient client = new HttpClient();
            client.BaseAddress = jiraUri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);

            var data = new
            {
                emailAddress = dashboardUserEmail
            };


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

            return new JiraUser { AccountId = "5fff143cf7ea2a0107ff9f87", DisplayName = "dirk.kaiser@clear.bible", EmailAddress = "dirk.kaiser@clear.bible" };
        }



    }
}
