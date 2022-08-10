using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

namespace ClearDashboard.WebApiParatextPlugin.Features.Versification
{
    public class GetVersificationBooksByParatextIdQueryHandler :
        IRequestHandler<GetVersificationAndBookIdByParatextPluginIdQuery, RequestResult<VersificationBookIds>>
    {
        private readonly ILogger<GetVersificationBooksByParatextIdQueryHandler> _logger;
        private readonly MainWindow _mainWindow;

        public GetVersificationBooksByParatextIdQueryHandler(ILogger<GetVersificationBooksByParatextIdQueryHandler> logger,
            MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }

        public Task<RequestResult<VersificationBookIds>> Handle(GetVersificationAndBookIdByParatextPluginIdQuery request, CancellationToken cancellationToken)
        {
            //var data = _mainWindow.GetVersificationAndBooksForProject(request.ParatextProjectId);

            var queryResult = new RequestResult<VersificationBookIds>();

            //try
            //{
            //    queryResult.Data = data;
            //}
            //catch (Exception ex)
            //{
            //    queryResult.Success = false;
            //    queryResult.Message = ex.Message;
            //}

            return Task.FromResult(queryResult);
        }
    }
}
