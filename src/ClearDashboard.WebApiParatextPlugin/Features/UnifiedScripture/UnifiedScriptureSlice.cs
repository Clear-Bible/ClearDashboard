using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using ParaTextPlugin.Data.Features;
using ParaTextPlugin.Data.Features.UnifiedScripture;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.UnifiedScripture
{
    public class GetUsxQueryHandler : IRequestHandler<GetUsxQuery, QueryResult<string>>
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

        public Task<QueryResult<string>> Handle(GetUsxQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new QueryResult<string>(string.Empty);
            try
            {
                queryResult.Data = _project.GetUSX(_verseRef.BookNum);
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }
            return Task.FromResult(queryResult);
        }
    }


    public class GetUsfmQueryHandler : IRequestHandler<GetUsfmQuery, QueryResult<string>>
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

        public Task<QueryResult<string>> Handle(GetUsfmQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new QueryResult<string>(string.Empty);
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
