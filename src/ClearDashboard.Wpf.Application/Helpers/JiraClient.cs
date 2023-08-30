using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class JiraClient
    {
        private static string _ProjectKey = "DUF";
        private static string _jsonTemplate = "{\"fields\": {\"project\": {\"key\": \"[[PROJECT]]\"},\"summary\": \"[[SUMMARY]]\",\"issuetype\": {\"name\": \"Task\"},\"reporter\": {\"accountId\": \"[[ACCOUNTID]]\",\"emailAddress\": \"[[EMAIL]]\",\"displayName\": \"[[EMAIL]]\"},\"description\": {\"content\": [{\"content\": [{\"text\": \"[[TEXTHERE]]\",\"type\": \"text\"}],\"type\": \"paragraph\"}],\"type\": \"doc\",\"version\": 1},\"labels\": [\"[[LABEL]]\"]}}";


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
            string severityText, JiraUser jiraUser)
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
                fields = new
                {
                    issuetype = new { id = 10002 /*Task*/ },
                    summary = jiraTitle,
                    project = new { key = _ProjectKey /*Key of your project*/},
                    description = new
                    {
                        version = 1,
                        type = "doc",
                        content = new[] {
                            new {
                                type = "paragraph",
                                content = new []{
                                    new {
                                        type = "text",
                                        text =  "Summary of the problem"
                                    }
                                }
                            }
                        }
                    },
                    reporter = new
                    {
                        accountId = jiraUser.AccountId,
                        emailAddress = jiraUser.EmailAddress,
                        displayName = jiraUser.DisplayName
                    },
                    labels = new[] { "bugfix" }
                }
            };

            //var data = new
            //{
            //    fields = new
            //    {
            //        issuetype = new { id = 10002 /*Task*/ },
            //        summary = jiraTitle,
            //        project = new { key = _ProjectKey /*Key of your project*/},
            //        description = new
            //        {
            //            version = 1,
            //            type = "doc",
            //            content = new[] {
            //                new {
            //                    type = "paragraph",
            //                    content = new []{
            //                        new {
            //                            type = "text",
            //                            text =  "Summary of the problem"
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //};



            var result = await client.PostAsJsonAsync($"/rest/api/3/issue", data);
            if (result.StatusCode == System.Net.HttpStatusCode.Created)
                return await result.Content.ReadFromJsonAsync<JiraTicketResponse>();
            else
                throw new Exception(await result.Content.ReadAsStringAsync());
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
            Uri jiraUri = new Uri( Settings.Default.JiraBaseUrl);
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

        public static Task<JiraUser> GetUserByEmail(List<JiraUser> jiraUsersList, DashboardUser dashboardUser)
        {
            var user = jiraUsersList.FirstOrDefault(x => x.EmailAddress == dashboardUser.Email);

            if (user != null)
                return Task.FromResult(user);
            else
            {
                // create a new user in Jira
                return CreateJiraUser(dashboardUser.Email);
            }

            return null;
        }


        /// <summary>
        /// Create a new Jira User using the email address from the dashboard user
        /// </summary>
        /// <param name="dashboardUserEmail"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task<JiraUser> CreateJiraUser(string? dashboardUserEmail)
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

            var result = await client.PostAsJsonAsync($"/rest/api/3/user", data);
            if (result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return await result.Content.ReadFromJsonAsync<JiraUser>();
            }
            
            return new JiraUser { AccountId = "5fff143cf7ea2a0107ff9f87", DisplayName = "dirk.kaiser@clear.bible", EmailAddress = "dirk.kaiser@clear.bible" };
        }

        

    }
}
