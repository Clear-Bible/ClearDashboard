
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
    public class GetWordAnalysesQueryHandler : IRequestHandler<GetWordAnalysesQuery, RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>>
    {
        private readonly ILogger<GetWordAnalysesQueryHandler> _logger;
        private readonly IWordAnalysesObtainable _wordAnalysesObtainable;

        public GetWordAnalysesQueryHandler(ILogger<GetWordAnalysesQueryHandler> logger, IWordAnalysesObtainable wordAnalysesObtainable)
        {
            _logger = logger;
			_wordAnalysesObtainable = wordAnalysesObtainable;
        }
        public Task<RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>> Handle(GetWordAnalysesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var wordAnalyses = _wordAnalysesObtainable.GetWordAnalyses(request.ProjectId);

                return Task.FromResult(new RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>(
					wordAnalyses)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving word analyses!");
                return Task.FromResult(new RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>(
                    null,
                    false,
                    ex.Message)
                );
            }
        }

    }
}
