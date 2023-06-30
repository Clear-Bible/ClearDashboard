using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;

namespace ClearDashboard.Wpf.Application.Services
{
    public class CollaborationHttpClientServices
    {
        #region Member Variables   

        private readonly CollaborationClient _collaborationClient;
        private ILogger? _logger;

        #endregion //Member Variables


        #region Constructor

        public CollaborationHttpClientServices(CollaborationClient collaborationClient)
        {
            _collaborationClient = collaborationClient;
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
                _logger = IoC.Get<ILogger<CollaborationHttpClientServices>>();
            }
        }

        #region GET Requests

        /// <summary>
        /// Gets a list of all the GitLab projects
        /// </summary>
        /// <returns></returns>
        public async Task<CollaborationUser> GetUserExistsById(int userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");

            try
            {
                var response = await _collaborationClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var user = JsonSerializer.Deserialize<CollaborationUser>(result);
                if (user is null)
                {
                    return new CollaborationUser { UserId = -1 };
                }
                return user;

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return new CollaborationUser { UserId = -1 };
        }


        public async Task<CollaborationUser> GetUserExistsByEmail(string email)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/");

            try
            {
                var response = await _collaborationClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var users = JsonSerializer.Deserialize<List<CollaborationUser>>(result);
                if (users is null)
                {
                    return new CollaborationUser { UserId = -1 };
                }

                var user = users.FirstOrDefault(u => u.RemoteEmail == email);
                if (user is null)
                {
                    return new CollaborationUser { UserId = -1 };
                }

                return user;

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return new CollaborationUser { UserId = -1 };
        }

        public async Task<DashboardUser> GetDashboardUserExistsById(Guid userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/dashboardusers/{userId}");

            try
            {
                var response = await _collaborationClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var user = JsonSerializer.Deserialize<DashboardUser>(result);
                if (user is null)
                {
                    return new DashboardUser { Id = Guid.Empty };
                }
                return user;

            }
            catch (Exception e)
            {
                return new DashboardUser { Id = Guid.Empty };
            }
        }

        public async Task<DashboardUser> GetDashboardUserExistsByEmail(string email)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/dashboardusers/search/{email}");

            try
            {
                var response = await _collaborationClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var user = JsonSerializer.Deserialize<DashboardUser>(result);
                if (user is null)
                {
                    return new DashboardUser { Id = Guid.Empty };
                }

                return user;

            }
            catch (Exception e)
            {
                return new DashboardUser { Id = Guid.Empty };
            }
        }

        public async Task<List<DashboardUser>> GetAllDashboardUsers()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/dashboardusers");

            try
            {
                var response = await _collaborationClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var users = JsonSerializer.Deserialize<List<DashboardUser>>(result);
                if (users is null)
                {
                    return new List<DashboardUser>();
                }
                return users;

            }
            catch (Exception e)
            {
                return new List<DashboardUser>();
            }
        }

        #endregion // GET Requests

        #region DELETE Requests

        public async Task<bool> DeleteDashboardUserExistsById(Guid userId)
        {
            try
            {
                var response = await _collaborationClient.Client.DeleteAsync($"/api/dashboardusers/{userId}");
                response.EnsureSuccessStatusCode();

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }



        #endregion // DELETE Requests


        #region POST Requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewUser(GitLabUser user, string accessToken)
        {
            var encryptedPassword = Encryption.Encrypt(user.Password);
            var encryptedPersonalAccessToken = Encryption.Encrypt(accessToken);

            var collaborationUser = new CollaborationUser
            {
                UserId = user.Id,
                RemoteUserName = user.UserName,
                GroupName = user.Organization,
                NamespaceId = user.NamespaceId,
                RemoteEmail = user.Email,
                RemotePersonalAccessToken = encryptedPersonalAccessToken,
                RemotePersonalPassword = encryptedPassword,
            };

            string jsonUser = JsonSerializer.Serialize(collaborationUser);
            var content = new System.Net.Http.StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var response = await _collaborationClient.Client.PostAsync("/api/users", content);
                response.EnsureSuccessStatusCode();

                return true;

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return false;
        }

        public async Task<bool> CreateNewDashboardUser(DashboardUser user)
        {
            string jsonUser = JsonSerializer.Serialize(user);
            var content = new System.Net.Http.StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var response = await _collaborationClient.Client.PostAsync("/api/dashboardusers", content);
                response.EnsureSuccessStatusCode();

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }


        #endregion // POST Requests

        #endregion // Methods
    }
}
