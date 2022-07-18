using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.ReferenceUsfm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.ReferenceUsfm
{
    public class GetReferenceUsfmQueryHandler : IRequestHandler<GetReferenceUsfmQuery, RequestResult<DataAccessLayer.Models.Common.ReferenceUsfm>>
    {
        private readonly ILogger<GetReferenceUsfmQueryHandler> _logger;
        private readonly MainWindow _mainWindow;

        public GetReferenceUsfmQueryHandler(ILogger<GetReferenceUsfmQueryHandler> logger, MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }
        public Task<RequestResult<DataAccessLayer.Models.Common.ReferenceUsfm>> Handle(GetReferenceUsfmQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<DataAccessLayer.Models.Common.ReferenceUsfm>(new DataAccessLayer.Models.Common.ReferenceUsfm());
            var result = _mainWindow.GetReferenceUSFM(request.Id);
            queryResult.Data = result;
            if (result.Name == "")
            {
                _logger.LogError($"GetReferenceUsfmQueryHandler - Unable to find this Id: {request.Id}");
                queryResult.Success = false;
                queryResult.Message = "Unable to find this Id";
            }

            return Task.FromResult(queryResult);
        }

    }
}
