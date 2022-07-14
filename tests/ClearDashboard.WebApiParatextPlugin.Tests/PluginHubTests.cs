using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using Microsoft.AspNet.SignalR.Client;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class PluginHubTests : TestBase
    {
      
        private List<string> _messages = new List<string>();
       

        public PluginHubTests(ITestOutputHelper output): base(output)
        {
           
        }

        [Fact]
        public async Task ReceiveMessageTestAsync()
        {
            try
            {
                await StartParatextAsync();

                var connection = new HubConnection("http://localhost:9000/signalr");

                var hubProxy = connection.CreateHubProxy("Plugin");
                hubProxy.On<string, string>("send", (name, msg) =>
                {
                    var message = $"{name} - {msg}";
                    _messages.Add(message);
                    Output.WriteLine(message);
                });


                hubProxy.On<string>("sendVerse", (verse) =>
                {
                    Assert.NotEmpty(verse);
                    Output.WriteLine($"Verse returned: {verse}");

                });

                hubProxy.On<ParatextProject>("sendProject", (project) =>
                {
                    Assert.NotNull(project);
                    Output.WriteLine($"Returned project: {project.ShortName}");

                });

                await connection.Start();

                var numberOfMessages = 10;

                for (var i = 1; i <= numberOfMessages; i++)
                {
                    _ = await hubProxy.Invoke<string>("ping", "Message", i);
                }

                Output.WriteLine($"Received {_messages.Count} messages.");

                Assert.NotEmpty(_messages);
                Assert.Equal(numberOfMessages, _messages.Count);
            }
            finally
            {
                await StopParatextAsync();
            }
        }
        
    }
}