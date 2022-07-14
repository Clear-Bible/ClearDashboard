using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetAllTokenizedCorpusIdsByCorpusIdQueryHandler : IRequestHandler<
        GetAllTokenizedCorpusIdsByCorpusIdQuery,
        RequestResult<IEnumerable<TokenizedCorpusId>>>
    {
        public Task<RequestResult<IEnumerable<TokenizedCorpusId>>>
            Handle(GetAllTokenizedCorpusIdsByCorpusIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: query TokenizedCorpus table by command.CorpusId and return enumerable of all TokenizedCorpus.Id.
            if (command.CorpusId.Id.Equals(new System.Guid("ca761232ed4211cebacd00aa0057b223")))
            {
                return Task.FromResult(
                    new RequestResult<IEnumerable<TokenizedCorpusId>>
                    (result: new List<TokenizedCorpusId>()
                    {   
                        new TokenizedCorpusId(new Guid()),
                        new TokenizedCorpusId(new Guid())
                    },
                    success: true,
                    message: "successful result from test"));
            }
            return Task.FromResult(
                new RequestResult<IEnumerable<TokenizedCorpusId>>
                (result: new List<TokenizedCorpusId>(),
                success: true,
                message: "successful result from test"));
        }
    }


}
