using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.CheckUsfm
{
    public class GetCheckUsfmQueryHandler : IRequestHandler<GetCheckUsfmQuery, RequestResult<List<UsfmError>>>
    {
        private readonly ILogger<GetCheckUsfmQueryHandler> _logger;
        private readonly MainWindow _mainWindow;

        public GetCheckUsfmQueryHandler(ILogger<GetCheckUsfmQueryHandler> logger, MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }
        public Task<RequestResult<List<UsfmError>>> Handle(GetCheckUsfmQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<List<UsfmError>>(new List<UsfmError>());
            var result = _mainWindow.GetCheckForUsfmErrors(request.Id);
            queryResult.Data = result;
            queryResult.Success = true;

            return Task.FromResult(queryResult);
        }
    }
}
