using System.Net.Http;

namespace ClearDashboard.Wpf.Application.Services
{
    public class MySqlClient
    {
        public MySqlClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }
}
