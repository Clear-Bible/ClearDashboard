using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using HttpClientToCurl;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using JsonSerializer = System.Text.Json.JsonSerializer;
using StringContent = System.Net.Http.StringContent;

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
        public async Task<bool> CheckForExistingUser(string userName, string email)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/users");
            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                List<GitLabUser> list = JsonSerializer.Deserialize<List<GitLabUser>>(result)!;
                // sort the list
                list = list.OrderBy(s => s.Name).ToList();
                // check both username & email fields
                var found = list.FirstOrDefault(s => s.UserName == userName || s.Email == email);
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


        public async Task<List<GitLabProject>> GetProjectForUser(CollaborationConfiguration user)
        {
            List<GitLabProject> list = new();

            GitAccessToken accessToken = new();
            var request = new HttpRequestMessage(HttpMethod.Get, $"users/{user.UserId}/projects");

            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                
                list = JsonSerializer.Deserialize<List<GitLabProject>>(result)!;
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

        public async Task<List<GitLabProjectUsers>> GetUsersForProject(CollaborationConfiguration collaborationConfiguration, int projectId)
        {
            List<GitLabProjectUsers> list = new();

            GitAccessToken accessToken = new();
            var request = new HttpRequestMessage(HttpMethod.Get, $"projects/{projectId}/users");

            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                list = JsonSerializer.Deserialize<List<GitLabProjectUsers>>(result)!;
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

        #endregion // GET Requests


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
            //content.Add(new StringContent(Uri.EscapeDataString($"{firstName} {lastName}")), "name");
            content.Add(new StringContent($"{firstName} {lastName}"), "name");
            content.Add(new StringContent(username), "username");
            content.Add(new StringContent(email), "email");
            content.Add(new StringContent(password), "password");
            content.Add(new StringContent(organization), "organization");
            content.Add(new StringContent("true"),"skip_confirmation");
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


        /// <summary>
        /// Generates a personal access token for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> GeneratePersonalAccessToken(GitLabUser user)
        {
            GitAccessToken accessToken = new();
            var request = new HttpRequestMessage(HttpMethod.Post, $"users/{user.Id}/personal_access_tokens");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent($"{user.UserName}-AccessToken"), "name");
            content.Add(new StringContent($"{user.Id}"), "user_id");
            content.Add(new StringContent("api"), "scopes");
            request.Content = content;

            try
            {
                //var curl = _gitLabClient.Client.GenerateCurlInString(request);

                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                accessToken = JsonSerializer.Deserialize<GitAccessToken>(result)!;
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return accessToken.Token;
        }

        public async Task<string> CreateNewProjectForUser(GitLabUser user, string projectName, string projectDescription)
        {
            GitAccessToken accessToken = new();
            var request = new HttpRequestMessage(HttpMethod.Post, $"projects");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent($"{user.Id}"), "user_id");
            content.Add(new StringContent($"{projectName}"), "name");
            // namespace_id: needed to point to the user that this project should fall under
            content.Add(new StringContent($"{user.NamespaceId}"), "namespace_id"); 
            content.Add(new StringContent($"{projectDescription}"), "description");
            request.Content = content;

            try
            {
                //var curl = _gitLabClient.Client.GenerateCurlInString(request);

                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();

                // todo

                //accessToken = JsonSerializer.Deserialize<GitAccessToken>(result)!;
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return accessToken.Token;
        }




        #endregion // POST Requests

        #endregion // Methods
    }
}
