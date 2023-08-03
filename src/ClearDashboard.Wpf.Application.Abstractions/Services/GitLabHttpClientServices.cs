using Autofac.Features.OwnedInstances;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using StringContent = System.Net.Http.StringContent;

namespace ClearDashboard.Wpf.Application.Services
{
    public class GitLabHttpClientServices
    {
        #region Member Variables   

        private readonly GitLabClient _gitLabClient;
        private ILogger? _logger;

        #endregion //Member Variables

        #region Public Properties



        #endregion //Public Properties


        #region Constructor

        public GitLabHttpClientServices(GitLabClient gitLabClient)
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
                _logger = IoC.Get<ILogger<GitLabHttpClientServices>>();
            }
        }

        #region GET Requests

        /// <summary>
        /// Gets a list of all the GitLab projects
        /// </summary>
        /// <returns></returns>
        public async Task<List<GitLabProject>> GetAllProjects()
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


            var list = new List<GitLabProject>();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/projects");
            var response = await _gitLabClient.Client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            list = JsonSerializer.Deserialize<List<GitLabProject>>(result)!;

            return list;
        }


        /// <summary>
        /// Get all of the GitLab Groups that represent the various
        /// organizations
        /// </summary>
        /// <returns></returns>
        public async Task<List<GitLabGroup>> GetAllGroups()
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));

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

        public async Task<List<GitUser>> GetAllUsers()
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


            var list = new List<GitUser>();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/users");
            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                list = JsonSerializer.Deserialize<List<GitUser>>(result)!;
                list = list.Where(c => c.Organization != "").ToList();

                // sort the list
                list = list.OrderBy(s => s.Organization).ThenBy(s => s.Name).ToList();
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
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


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


        public async Task<List<GitLabProject>> GetProjectsForUser(CollaborationConfiguration user)
        {
            List<GitLabProject> list = new();

            var request = new HttpRequestMessage(HttpMethod.Get, $"projects");

            GitLabClient newClient = _gitLabClient;
            newClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.RemotePersonalAccessToken);

            try
            {
                var response = await newClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                list = JsonSerializer.Deserialize<List<GitLabProject>>(result)!;
                // sort the list
                list = list.OrderBy(s => s.Name).ToList();

                // remove non-project repos
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!(list[i].Name.StartsWith("P_") && list[i].Name.Length == 38))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return list;
        }


        public async Task<List<GitLabProject>> GetProjectsForUserWhereOwner(CollaborationConfiguration user)
        {
            List<GitLabProject> list = new();

            var request = new HttpRequestMessage(HttpMethod.Get, $"projects");

            GitLabClient newClient = _gitLabClient;
            newClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.RemotePersonalAccessToken);

            try
            {
                var response = await newClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                list = JsonSerializer.Deserialize<List<GitLabProject>>(result)!;
                // sort the list
                list = list.OrderBy(s => s.Name).ToList();

                // remove non-project repos
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!(list[i].Name.StartsWith("P_") && list[i].Name.Length == 38))
                    {
                        list.RemoveAt(i);
                    }
                }

                // remove projects for which we are not the owner
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Permissions.ProjectAccess.AccessLevel < 50)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return list;
        }

        public async Task<List<GitLabProjectUser>> GetUsersForProject(CollaborationConfiguration collaborationConfiguration, int projectId)
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));

            List<GitLabProjectUser> list = new();

            var request = new HttpRequestMessage(HttpMethod.Get, $"projects/{projectId}/members");

            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                list = JsonSerializer.Deserialize<List<GitLabProjectUser>>(result)!;
                // sort the list
                list = list.OrderBy(s => s.Name).ToList();

                foreach (var item in list)
                {
                    if (item.AccessLevel == 50)
                    {
                        item.IsOwner = true;
                    }
                }


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
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));

            var user = new GitLabUser();
            var request = new HttpRequestMessage(HttpMethod.Post, "users");

            var content = new MultipartFormDataContent();
            //content.Add(new StringContent(Uri.EscapeDataString($"{firstName} {lastName}")), "name");
            content.Add(new StringContent($"{firstName} {lastName}"), "name");
            content.Add(new StringContent(username), "username");
            content.Add(new StringContent(email), "email");
            content.Add(new StringContent(password), "password");
            content.Add(new StringContent(organization), "organization");
            content.Add(new StringContent("true"), "skip_confirmation");
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
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));

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

        public async Task<GitLabProject> CreateNewProjectForUser(GitLabUser user, string projectName, string projectDescription)
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


            GitLabProject project = new();
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
                project = JsonSerializer.Deserialize<GitLabProject>(result)!;
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return project;
        }

        public async Task<object> AddUserToProject(GitUser user, GitLabProject selectedProject, PermissionLevel permissionLevel)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"projects/{selectedProject.Id}/members");

            var content = new MultipartFormDataContent();
            //content.Add(new StringContent(Uri.EscapeDataString($"{firstName} {lastName}")), "name");
            content.Add(new StringContent(user.Id.ToString()), "user_id");
            switch (permissionLevel)
            {
                case PermissionLevel.ReadOnly:
                    content.Add(new StringContent("30"), "access_level");
                    break;
                case PermissionLevel.ReadWrite:
                    content.Add(new StringContent("40"), "access_level");
                    break;
                case PermissionLevel.Owner:
                    content.Add(new StringContent("50"), "access_level");
                    break;
                default:
                    content.Add(new StringContent("40"), "access_level");
                    break;
            }

            request.Content = content;

            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));

            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();

                var returnUser = JsonSerializer.Deserialize<GitLabAddUserToProject>(result);

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return user;
        }

        public async Task RemoveUserFromProject(GitLabProjectUser selectedCurrentUser, GitLabProject selectedProject)
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


            var request = new HttpRequestMessage(HttpMethod.Delete, $"projects/{selectedProject.Id}/members/{selectedCurrentUser.Id}");

            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }
        }

        #endregion // POST Requests


        #region DELETE Requests

        public async Task<bool> DeleteUser(GitLabProjectUser user)
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _gitLabClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


            var request = new HttpRequestMessage(HttpMethod.Delete, $"users/{user.Id}");

            try
            {
                var response = await _gitLabClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                if ((int)response.StatusCode == 204)
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return false;
        }

        #endregion // Delete requests



        #endregion // Methods
    }
}
