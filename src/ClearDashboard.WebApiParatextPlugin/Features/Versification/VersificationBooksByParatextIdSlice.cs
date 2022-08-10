using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;

namespace ClearDashboard.WebApiParatextPlugin.Features.Versification
{
    public class GetVersificationBooksByParatextIdQueryHandler :
        IRequestHandler<GetVersificationAndBookIdByParatextPluginIdQuery, RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
    {
        private readonly ILogger<GetVersificationBooksByParatextIdQueryHandler> _logger;
        private readonly MainWindow _mainWindow;

        public GetVersificationBooksByParatextIdQueryHandler(ILogger<GetVersificationBooksByParatextIdQueryHandler> logger,
            MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }

        public Task<RequestResult<(ScrVers versification, IEnumerable<string> bookAbbreviations)>> Handle(GetVersificationAndBookIdByParatextPluginIdQuery request, CancellationToken cancellationToken)
        {
            var data = _mainWindow.GetVersificationAndBooksForProject(request.ParatextProjectId);

            var queryResult = new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>();

            try
            {
                queryResult.Data = data;
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }
    }
}
