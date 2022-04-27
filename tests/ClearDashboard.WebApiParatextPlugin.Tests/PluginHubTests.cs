using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class PluginHubTests
    {
        private readonly ITestOutputHelper _output;
        private string? _message;

        public PluginHubTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Timeout = 15 * 1000)]
        public async Task ReceiveMessageTest()
        {
            var connection = new HubConnection("http://localhost:9000/signalr");

            var hubProxy = connection.CreateHubProxy("Plugin");
            hubProxy.On<string, string>("send", (name, msg) =>
            {
                _message = $"{name} - {msg}";
                _output.WriteLine(_message);
            });

            await connection.Start();
            Console.Read();
        }
    }
}