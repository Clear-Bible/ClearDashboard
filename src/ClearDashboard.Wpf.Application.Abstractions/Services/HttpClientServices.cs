using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using HttpClientToCurl;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ClearDashboard.Wpf.Application.Services
{
    public class HttpClientServices
    {
        #region Member Variables   

        private readonly GitLabClient _gitLabClient;
        private ILogger? _logger;

        #endregion //Member Variables


        #region Constructor

        public HttpClientServices(GitLabClient gitLabClient)
        {
            _gitLabClient = gitLabClient;
        }

        #endregion //Constructor


        #region Methods

        /// <summary>
        /// Sets up the logger if it isn't already
        /// </summary>
        private void WireUpLogger()
        {
            if (_logger is null)
            {
                _logger = IoC.Get<ILogger<HttpClientServices>>();
            }
        }

        #region GET Requests

        /// <summary>
        /// Gets a list of all the GitLab projects
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllProjects()
        {
            var list = new List<string>();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/projects");
            var response = await _gitLabClient.Client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(result);



            return list;
        }


        /// <summary>
        /// Get all of the GitLab Groups that represent the various
        /// organizations
        /// </summary>
        /// <returns></returns>
        public async Task<List<GitLabGroup>> GetAllGroups()
        {
            var list = new List<GitLabGroup>();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/groups");
            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                list = JsonSerializer.Deserialize<List<GitLabGroup>>(result)!;
                // sort the list
                list = list.OrderBy(s => s.Name).ToList();
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return list;
        }

        /// <summary>
        /// Checks to see if the user already exists on GitLab
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckForExistingUser(string userName)
        {
            var list = new List<GitLabUser>();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/users");
            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                list = JsonSerializer.Deserialize<List<GitLabUser>>(result)!;
                // sort the list
                list = list.OrderBy(s => s.Name).ToList();

                var found = list.FirstOrDefault(s => s.UserName == userName);
                if (found is null)
                {
                    return false;
                }
                
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return true;
        }

        #endregion


        #region POST Requests

        /// <summary>
        /// Creates a new user in GitLab
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="organization"></param>
        /// <returns></returns>
        public async Task<GitLabUser> CreateNewUser(string firstName, string lastName, string username, string password,
            string email, string organization)
        {
            var user = new GitLabUser();
            var request = new HttpRequestMessage(HttpMethod.Post, "users");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Uri.EscapeDataString($"{firstName} {lastName}")), "name");
            content.Add(new StringContent(username), "username");
            content.Add(new StringContent(email), "email");
            content.Add(new StringContent(password), "password");
            content.Add(new StringContent(organization), "organization");
            request.Content = content;

            try
            {
                //var curl = _gitLabClient.Client.GenerateCurlInString(request);

                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                user = JsonSerializer.Deserialize<GitLabUser>(result)!;


                //using (var stream = await response.Content.ReadAsStreamAsync())
                //{
                //    using (var streamReader = new StreamReader(stream))
                //    {
                //        using (var jsonTextReader = new JsonTextReader(streamReader))
                //        {
                //            var customer = JsonSerializer.Deserialize<GitLabUser>(jsonTextReader)!;

                //            // do something with the customer
                //        }
                //    }
                //}

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return user;
        }

        #endregion

        #endregion // Methods
    }
}
