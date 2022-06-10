using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;

namespace ClearDashboard.WebApiParatextPlugin.Features.UnifiedScripture
{
    public class GetUsxQueryHandler : IRequestHandler<GetUsxQuery, RequestResult<UsxObject>>
    {
        private readonly IVerseRef _verseRef;
        private readonly IProject _project;
        private readonly ILogger<GetUsxQueryHandler> _logger;

        public GetUsxQueryHandler(IVerseRef verseRef, ILogger<GetUsxQueryHandler> logger, IProject project)
        {
            _verseRef = verseRef;
            _logger = logger;
            _project = project;
        }

        public Task<RequestResult<UsxObject>> Handle(GetUsxQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<UsxObject>();
            try
            {
                UsxObject usxObject = new();
                usxObject.USX = _project.GetUSX(request.BookNumber ?? _verseRef.BookNum);
                queryResult.Data = usxObject;
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }
            return Task.FromResult(queryResult);
        }
    }


    public class GetUsfmQueryHandler : IRequestHandler<GetUsfmQuery, RequestResult<string>>
    {
        private readonly IVerseRef _verseRef;
        private readonly IProject _project;
        private readonly ILogger<GetUsfmQueryHandler> _logger;

        public GetUsfmQueryHandler(IVerseRef verseRef, ILogger<GetUsfmQueryHandler> logger, IProject project)
        {
            _verseRef = verseRef;
            _logger = logger;
            _project = project;
        }

        public Task<RequestResult<string>> Handle(GetUsfmQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<string>(string.Empty);
            try
            {
                queryResult.Data = _project.GetUSFM(_verseRef.BookNum);
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
