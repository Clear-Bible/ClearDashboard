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
        RequestResult<IEnumerable<TokenizedTextCorpusId>>>
    {
        public Task<RequestResult<IEnumerable<TokenizedTextCorpusId>>>
            Handle(GetAllTokenizedCorpusIdsByCorpusIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: query TokenizedCorpus table by command.CorpusId and return enumerable of all TokenizedCorpus.Id.
            if (command.CorpusId.Id.Equals(new System.Guid("ca761232ed4211cebacd00aa0057b223")))
            {
                return Task.FromResult(
                    new RequestResult<IEnumerable<TokenizedTextCorpusId>>
                    (result: new List<TokenizedTextCorpusId>()
                    {   
                        new TokenizedTextCorpusId(new Guid()),
                        new TokenizedTextCorpusId(new Guid())
                    },
                    success: true,
                    message: "successful result from test"));
            }
            return Task.FromResult(
                new RequestResult<IEnumerable<TokenizedTextCorpusId>>
                (result: new List<TokenizedTextCorpusId>(),
                success: true,
                message: "successful result from test"));
        }
    }


}
