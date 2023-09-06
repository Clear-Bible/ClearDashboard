
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Lexicon
{
    public class GetLexiconQueryHandler : IRequestHandler<GetLexiconQuery, RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>>
    {
        private readonly ILogger<GetLexiconQueryHandler> _logger;
        private readonly ILexiconObtainable _lexiconObtainable;

        public GetLexiconQueryHandler(ILogger<GetLexiconQueryHandler> logger, ILexiconObtainable lexiconObtainable)
        {
            _logger = logger;
            _lexiconObtainable = lexiconObtainable;
        }
        public Task<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>> Handle(GetLexiconQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var lexicon = _lexiconObtainable.GetLexicon();

                return Task.FromResult(new RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>(
                    lexicon)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lexicon!");
                return Task.FromResult(new RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>(
                    null,
                    false,
                    ex.Message)
                );
            }
        }

    }
}
