using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class PluginHubTests
    {
        private readonly ITestOutputHelper _output;
        private List<string> _messages = new List<string>();
        private Process _process;
        public PluginHubTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public async Task ReceiveMessageTest()
        {
            // NB:  to run this test, you must have Paratext running with the
            //      "Clear Dashboard Paratext Plugin" running.  For the test to pass
            //      you must click the "Send Message" button in the plugin UI at least once.

            var paratext = Process.GetProcessesByName("Paratext");

            var stopParatext = false;

            if (paratext.Length == 0)
            {
                _output.WriteLine("Starting Paratext.");
                _process = await StartParatext();
                stopParatext = true;
            }
            else
            {
                _process = paratext[0];
                _output.WriteLine("Paratext is already running.");
            }

            var connection = new HubConnection("http://localhost:9000/signalr");

            var hubProxy = connection.CreateHubProxy("Plugin");
            hubProxy.On<string, string>("send", (name, msg) =>
            {
                var message = $"{name} - {msg}";
                _messages.Add(message);
                _output.WriteLine(message);
            });

            await connection.Start();


            for (var i = 0; i < 100; i++)
            {

                var message = await hubProxy.Invoke<string>("ping", "Message", i);
            }
            

            //await Task.Delay(TimeSpan.FromSeconds(15));

            _output.WriteLine($"Received {_messages.Count} messages.");

            Assert.NotEmpty(_messages);
           // Console.Read();


           if (stopParatext)
           {
               _output.WriteLine("Stopping Paratext.");
               _process.Kill(true);
           }
        }

        private async Task<Process> StartParatext()
        {
            var paratextInstallDirictory = Environment.GetEnvironmentVariable("ParatextInstallDir");
            var process = Process.Start($"{paratextInstallDirictory}\\paratext.exe");

            await Task.Delay(TimeSpan.FromSeconds(2));

            return process;
        }
    }
}