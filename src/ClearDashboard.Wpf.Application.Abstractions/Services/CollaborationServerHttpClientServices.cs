using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;

namespace ClearDashboard.Wpf.Application.Services
{
    public class CollaborationServerHttpClientServices
    {
        #region Member Variables   

        private readonly CollaborationClient _collaborationClient;
        private ILogger? _logger;

        #endregion //Member Variables


        #region Constructor

        public CollaborationServerHttpClientServices(CollaborationClient collaborationClient)
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
                _logger = IoC.Get<ILogger<CollaborationServerHttpClientServices>>();
            }
        }

        #region GET Requests

        /// <summary>
        /// Gets a list of all the GitLab projects
        /// </summary>
        /// <returns></returns>
        public async Task<CollaborationUser> GetCollabUserExistsById(int userId)
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


        public async Task<CollaborationUser> GetCollabUserExistsByEmail(string email)
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
            var query = new Dictionary<string, string>()
            {
                ["api-version"] = "2.0",
            };
            var uri = QueryHelpers.AddQueryString($"/api/extendedusers/{userId.ToString()}", query);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

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
            var query = new Dictionary<string, string>()
            {
                ["api-version"] = "2.0",
            };
            var uri = QueryHelpers.AddQueryString($"/api/extendedusers/search/{email}", query);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

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
            var query = new Dictionary<string, string>()
            {
                ["api-version"] = "2.0",
            };
            var uri = QueryHelpers.AddQueryString($"/api/extendedusers", query);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

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
                var query = new Dictionary<string, string>()
                {
                    ["api-version"] = "2.0",
                };
                var uri = QueryHelpers.AddQueryString($"/api/extendedusers/{userId}", query);
                var response = await _collaborationClient.Client.DeleteAsync(uri);
                response.EnsureSuccessStatusCode();

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCollaborationUserById(int userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");

            try
            {
                var response = await _collaborationClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                if (result.Contains("Deleted Successfully"))
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

        #endregion //DELETE Requests

        #region PUT Requests

        public async Task<bool> UpdateDashboardUser(DashboardUser user)
        {
            string jsonUser = JsonSerializer.Serialize(user);
            var content = new System.Net.Http.StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                // add in the query to the URL
                var query = new Dictionary<string, string>()
                {
                    ["api-version"] = "2.0",
                };
                var uri = QueryHelpers.AddQueryString("/api/extendedusers", query);

                var response = await _collaborationClient.Client.PutAsync(uri, content);
                response.EnsureSuccessStatusCode();

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }



        #endregion // PUT Requests


        #region POST Requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewCollabUser(GitLabUser user, string accessToken)
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
                var query = new Dictionary<string, string>()
                {
                    ["api-version"] = "2.0",
                };
                var uri = QueryHelpers.AddQueryString("/api/extendedusers", query);

                var response = await _collaborationClient.Client.PostAsync(uri, content);
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
