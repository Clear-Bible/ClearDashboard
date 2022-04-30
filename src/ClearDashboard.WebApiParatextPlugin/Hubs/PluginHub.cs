using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.Data.Features.Project;
using ClearDashboard.ParatextPlugin.Data.Features.Verse;
using ClearDashboard.ParatextPlugin.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.WebApiParatextPlugin.Hubs
{

    [HubName("Plugin")]
    public class PluginHub : Hub
    {
        private readonly IMediator _mediator;
        public PluginHub()
        {
            // TODO:  Investigate how to get SignalR to inject the mediator for us.
            //        I really hate this tight coupling.
            _mediator = WebHostStartup.ServiceProvider.GetService<IMediator>();
        }
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        public void SendProject(Project project)
        {
            Clients.All.addMessage(project);
        }

        public void SendVerse(string verse)
        {
            Clients.All.addMessage(verse);
        }

        public override async Task OnConnected()
        {
            {
                var result = await _mediator.Send(new GetCurrentVerseCommand());
                if (result.Success)
                {
                    Clients.All.SendVerse(result.Data);
                }
            }

            {
                var result = await _mediator.Send(new GetCurrentProjectCommand());
                if (result.Success)
                {
                    Clients.All.SendProject(result.Data);
                }
            }
            await base.OnConnected();
        }
    }
}
