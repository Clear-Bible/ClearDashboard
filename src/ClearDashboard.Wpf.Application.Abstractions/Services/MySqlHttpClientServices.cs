using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public class MySqlHttpClientServices
    {
        #region Member Variables   

        private readonly MySqlClient _mySqlClient;
        private ILogger? _logger;

        #endregion //Member Variables

        #region Public Properties



        #endregion //Public Properties


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
        public async Task<List<string>> GetAllProjects()
        {
            var value = Encryption.Decrypt("IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");
            _mySqlClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));


            var list = new List<string>();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://gitlab.cleardashboard.org/api/v4/projects");
            var response = await _mySqlClient.Client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(result);



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
            _mySqlClient.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value.Replace("Bearer ", ""));

            var user = new GitLabUser();
            var request = new HttpRequestMessage(HttpMethod.Post, "users");

            var content = new MultipartFormDataContent();
            //content.Add(new StringContent(Uri.EscapeDataString($"{firstName} {lastName}")), "name");
            content.Add(new System.Net.Http.StringContent($"{firstName} {lastName}"), "name");
            content.Add(new System.Net.Http.StringContent(username), "username");
            content.Add(new System.Net.Http.StringContent(email), "email");
            content.Add(new System.Net.Http.StringContent(password), "password");
            content.Add(new System.Net.Http.StringContent(organization), "organization");
            content.Add(new System.Net.Http.StringContent("true"), "skip_confirmation");
            request.Content = content;

            try
            {
                //var curl = _gitLabClient.Client.GenerateCurlInString(request);

                var response = await _mySqlClient.Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                user = JsonSerializer.Deserialize<GitLabUser>(result)!;

            }
            catch (Exception e)
            {
                WireUpLogger();
                _logger?.LogError(e.Message, e);
            }

            return user;
        }


 

        #endregion // POST Requests

        #endregion // Methods
    }
}
