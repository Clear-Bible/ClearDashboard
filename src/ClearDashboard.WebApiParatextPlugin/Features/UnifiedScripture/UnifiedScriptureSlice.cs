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
    public class GetUsxQueryHandler : IRequestHandler<GetUsxQuery, RequestResult<StringObject>>
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

        public Task<RequestResult<StringObject>> Handle(GetUsxQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<StringObject>();
            try
            {
                StringObject stringObject = new();
                stringObject.StringData = _project.GetUSX(request.BookNumber ?? _verseRef.BookNum);
                queryResult.Data = stringObject;
                queryResult.Success = true;
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }
            return Task.FromResult(queryResult);
        }
    }


    public class GetUsfmQueryHandler : IRequestHandler<GetUsfmQuery, RequestResult<StringObject>>
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

        public Task<RequestResult<StringObject>> Handle(GetUsfmQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<StringObject>();
            try
            {
                StringObject stringObject = new();
                stringObject.StringData = _project.GetUSFM(request.BookNumber ?? _verseRef.BookNum);
                queryResult.Data = stringObject;
                queryResult.Success = true;
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
