using System.Net.Http;

namespace ClearDashboard.Wpf.Application.Services
{
    public class GitLabClient
    {
        public GitLabClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }
}
