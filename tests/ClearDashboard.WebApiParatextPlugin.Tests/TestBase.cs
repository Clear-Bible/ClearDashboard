using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }
        protected Process? Process { get; set; }
        protected bool StopParatextOnTestConclusion { get; set; }
        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider? _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }

        protected HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:9000/api/");
            return client;
        }

        protected virtual void SetupDependencyInjection()
        {
            Services.AddLogging();
        }

        protected async Task StartParatext()
        {
            var paratext = Process.GetProcessesByName("Paratext");

            if (paratext.Length == 0)
            {
                Output.WriteLine("Starting Paratext.");
                Process = await InternalStartParatext();
                StopParatextOnTestConclusion = true;

                var seconds = 2;
                Output.WriteLine($"Waiting for {seconds} seconds for Paratext to complete initialization.");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }
            else
            {
                Process = paratext[0];
                Output.WriteLine("Paratext is already running.");
            }


        }

        protected async Task StopParatext()
        {
            if (StopParatextOnTestConclusion)
            {
                Output.WriteLine("Stopping Paratext.");
                Process.Kill(true);

                Process = null;

                var seconds = 2;
                Output.WriteLine($"Waiting for {seconds} seconds for Paratext to stop.");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }
        }

        private async Task<Process> InternalStartParatext()
        {
            var paratextInstallDirectory = Environment.GetEnvironmentVariable("ParatextInstallDir");
            var process = Process.Start($"{paratextInstallDirectory}\\paratext.exe");

            return process;
        }
    }
}
