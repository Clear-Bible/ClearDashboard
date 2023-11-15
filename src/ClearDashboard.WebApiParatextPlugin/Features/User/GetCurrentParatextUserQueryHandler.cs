using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using ClearDashboard.WebApiParatextPlugin.Features.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.User
{
    public class
        GetCurrentParatextUserQueryHandler : IRequestHandler<GetCurrentParatextUserQuery, RequestResult<AssignedUser>>
    {
        private readonly IPluginHost _pluginHost;
        private readonly ILogger<GetCurrentProjectQueryHandler> _logger;

        public GetCurrentParatextUserQueryHandler(IPluginHost pluginHost, ILogger<GetCurrentProjectQueryHandler> logger)
        {
            _pluginHost = pluginHost;
            _logger = logger;
        }

        public Task<RequestResult<AssignedUser>> Handle(GetCurrentParatextUserQuery request,
            CancellationToken cancellationToken)
        {

            var name = _pluginHost.UserInfo.Name;
            var result = new RequestResult<AssignedUser>(new AssignedUser() {Name = name});

            if (string.IsNullOrEmpty(name))
            {
                result.Data.Name = "USER UNKNOWN";
                result.Message = "There are no users registered with Paratext";
                result.Success = false;
            }
            return Task.FromResult(result);
        }
    }

}
