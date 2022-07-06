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
    public class GetAllCorpusIdsQueryHandler : IRequestHandler<
        GetAllCorpusIdsQuery,
        RequestResult<IEnumerable<CorpusId>>>
    {
        public Task<RequestResult<IEnumerable<CorpusId>>>
            Handle(GetAllCorpusIdsQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: query Corpus table and return all ids

            return Task.FromResult(
                new RequestResult<IEnumerable<CorpusId>>
                (result: new List<CorpusId>() { 
                    new CorpusId(new Guid("ca761232ed4211cebacd00aa0057b223")),
                    new CorpusId(new Guid())
                },
                success: true,
                message: "successful result from test"));
        }
    }


}
