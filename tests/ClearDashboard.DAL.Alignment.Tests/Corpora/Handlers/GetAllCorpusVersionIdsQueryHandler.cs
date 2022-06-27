using MediatR;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearBible.Alignment.DataServices.Features.Corpora;
using System;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
{
    public class GetAllCorpusVersionIdsQueryHandler : IRequestHandler<
        GetAllCorpusVersionIdsQuery,
        RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>>
    {
        public Task<RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>>
            Handle(GetAllCorpusVersionIdsQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: query CorpusVersion table and return all ids

            return Task.FromResult(
                new RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>
                (result: new List<(CorpusVersionId corpusVersionId, CorpusId corpusId)>() { 
                    (new CorpusVersionId(new Guid("ca761232ed4211cebacd00aa0057b223"), DateTime.UtcNow), new CorpusId(new Guid())),
                    (new CorpusVersionId(new Guid("ca761232ed4211cebacd00aa0057b255"), DateTime.UtcNow), new CorpusId(new Guid()))
                },
                success: true,
                message: "successful result from test"));
        }
    }


}
