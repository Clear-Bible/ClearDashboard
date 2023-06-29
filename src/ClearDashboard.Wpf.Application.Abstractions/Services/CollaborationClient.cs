using System.Net.Http;

namespace ClearDashboard.Wpf.Application.Services
{
    public class CollaborationClient
    {
        public CollaborationClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }
}
