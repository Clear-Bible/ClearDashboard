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

namespace ClearDashboard.Wpf.Application.Services
{
    public class MySqlHttpClientServices
    {
        #region Member Variables   

        private readonly MySqlClient _mySqlClient;
        private ILogger? _logger;

        #endregion //Member Variables


        #region Constructor

        public MySqlHttpClientServices(MySqlClient mySqlClient)
        {
            _mySqlClient = mySqlClient;
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
                _logger = IoC.Get<ILogger<MySqlHttpClientServices>>();
            }
        }

        #region GET Requests

        /// <summary>
        /// Gets a list of all the GitLab projects
        /// </summary>
        /// <returns></returns>
        public async Task<MySqlUser> GetUserExistsById(int userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");

            try
            {
                var response = await _mySqlClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var user = JsonSerializer.Deserialize<MySqlUser>(result);
                if (user is null)
                {
                    return new MySqlUser { UserId = -1 };
                }
                return user;

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return new MySqlUser { UserId = -1 };
        }


        public async Task<MySqlUser> GetUserExistsByEmail(string email)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/");

            try
            {
                var response = await _mySqlClient.Client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();


                var users = JsonSerializer.Deserialize<List<MySqlUser>>(result);
                if (users is null)
                {
                    return new MySqlUser { UserId = -1 };
                }

                var user = users.FirstOrDefault(u => u.RemoteEmail == email);
                if (user is null)
                {
                    return new MySqlUser { UserId = -1 };
                }

                return user;

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return new MySqlUser { UserId = -1 };
        }

        #endregion // GET Requests


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

            var mySqlUser = new MySqlUser
            {
                UserId = user.Id,
                RemoteUserName = user.UserName,
                GroupName = user.Organization,
                NamespaceId = user.NamespaceId,
                RemoteEmail = user.Email,
                RemotePersonalAccessToken = encryptedPersonalAccessToken,
                RemotePersonalPassword = encryptedPassword,
            };

            string jsonUser = JsonSerializer.Serialize(mySqlUser);
            var content = new System.Net.Http.StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var response = await _mySqlClient.Client.PostAsync("/api/users", content);
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




        #endregion // POST Requests

        #endregion // Methods
    }
}
