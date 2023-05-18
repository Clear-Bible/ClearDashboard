using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public class HttpClientServices
    {
        private readonly GitLabClient _gitLabClient;

        public HttpClientServices(GitLabClient gitLabClient)
        {
            _gitLabClient = gitLabClient;
        }

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
    }
}
