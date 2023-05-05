using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Corpa
{
    public record GetBooksFromTokenizedCorpusQuery(string paratextGuid) : ProjectRequestQuery<List<string>>;

    public class GetBooksFromTokenizedCorpusQueryHandler : ProjectDbContextQueryHandler<GetBooksFromTokenizedCorpusQuery,
        RequestResult<List<string>>, List<string>>
    {
        private readonly IMediator _mediator;
        public GetBooksFromTokenizedCorpusQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetBooksFromTokenizedCorpusQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<List<string>>> GetDataAsync(GetBooksFromTokenizedCorpusQuery request, CancellationToken cancellationToken)
        {
            List<string> tokenizedBookList = new();

            try
            {
                var corpus = ProjectDbContext.Corpa.FirstOrDefault(c => c.ParatextGuid == request.paratextGuid);
                if (corpus != null)
                {
                    var tokenizedCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(t => t.CorpusId == corpus.Id);
                    var tokenComponents = ProjectDbContext.TokenComponents.Where(t => t.TokenizedCorpusId == tokenizedCorpus.Id);

                    tokenizedBookList = tokenComponents.Select(t => t.EngineTokenId.Substring(0, 3)).Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("An unexpected error occurred while determine which books are in a tokenized corpus.");
            }
            return new RequestResult<List<string>>(tokenizedBookList);
        }
    }
}
