using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Verse
{
    public class GetCurrentVerseQueryHandler : IRequestHandler<GetCurrentVerseQuery, RequestResult<string>>
    {
        private readonly IVerseRef _verseRef;
        private readonly ILogger<GetCurrentVerseQueryHandler> _logger;

        public GetCurrentVerseQueryHandler(IVerseRef verseRef, ILogger<GetCurrentVerseQueryHandler> logger)
        {
            _verseRef = verseRef;
            _logger = logger;
        }

        public Task<RequestResult<string>> Handle(GetCurrentVerseQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<string>(string.Empty);
            try
            {
                var verseId = _verseRef.BBBCCCVVV.ToString();
                if (verseId.Length < 8)
                {
                    verseId = verseId.PadLeft(9, '0');
                }

                queryResult.Data = verseId;

            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }
    }

    public class SetCurrentVerseCommandHandler : IRequestHandler<SetCurrentVerseCommand, RequestResult<string>>
    {
        private readonly IVerseRef _verseRef;
        private readonly ILogger<SetCurrentVerseCommandHandler> _logger;
        private readonly IProject _project;

        public SetCurrentVerseCommandHandler(IVerseRef verseRef, ILogger<SetCurrentVerseCommandHandler> logger,
            IProject project)
        {
            _project = project;
            _verseRef = verseRef;
            _logger = logger;
        }

        public Task<RequestResult<string>> Handle(SetCurrentVerseCommand request, CancellationToken cancellationToken)
        {
            //_project.


            return Task.FromResult(new RequestResult<string>(request.Verse));

            //var queryResult = new RequestResult<string>(string.Empty);
            //try
            //{
            //    var verseId = request.Verse;
            //    if (verseId.Length < 8)
            //    {
            //        verseId = verseId.PadLeft(9, '0');
            //    }

            //    _verseRef.BBBCCCVVV = new VerseRef(verseId);
            //}

        }
    }
}