using System;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Hubs
{

    [HubName("Plugin")]
    public class PluginHub : Hub
    {
        private readonly IMediator _mediator;
        private readonly IPluginLogger _logger;
        public PluginHub()
        {
            // TODO:  Investigate how to get SignalR to inject the mediator for us.
            //        I really hate this tight coupling.
            _mediator = WebHostStartup.ServiceProvider.GetService<IMediator>();
            _logger = WebHostStartup.ServiceProvider.GetService<IPluginLogger>();
        }
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        public async void SendProject(ParatextProject project)
        {
            Clients.All.addMessage(project);
        }

        public async void SendCurrentProject()
        {
            try
            {
                var result = await _mediator.Send(new GetCurrentProjectQuery());
                if (result.Success)
                {
                    _logger.AppendText(Color.DarkOrange, $"Sending project - {result.Data?.ShortName}");
                    Clients.All.SendProject(result.Data);
                }
            }
            catch (Exception e)
            {
                _logger.AppendText(Color.Red, e.Message);
            }
        }

        public void SendVerse(string verse)
        {
            Clients.All.addMessage(verse);
        }

        public void SendPluginClosing(PluginClosing pluginClosing)
        {
            Clients.All.addMessage(pluginClosing);
        }

        public void SendTextCollections(List<TextCollection> textCollections)
        {
            Clients.All.addMessage(textCollections);
        }

        public void Ping(string message, int index)
        {
            _logger.AppendText(Color.CornflowerBlue, $"Received ping - {message}: {index}");
            Clients.All.Send(message, index.ToString());
        }

        public override async Task OnConnected()
        {
            {
                _logger.AppendText(Color.DarkOrange, $"New client connected - {Context.ConnectionId}");
                var result = await _mediator.Send(new GetCurrentVerseQuery());
                if (result.Success)
                {
                    _logger.AppendText(Color.DarkOrange, $"Sending verse - {result.Data}");
                    Clients.All.SendVerse(result.Data);
                }
            }

            {
                var result = await _mediator.Send(new GetCurrentProjectQuery());
                if (result.Success)
                {
                    _logger.AppendText(Color.DarkOrange, $"Sending project - {result.Data?.ShortName}");
                    Clients.All.SendProject(result.Data);
                }
            }

            await base.OnConnected();
        }
    }
}
