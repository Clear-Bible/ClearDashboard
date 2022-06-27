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
    public class GetAllTokenizedCorpusIdsByCorpusVersionIdQueryHandler : IRequestHandler<
        GetAllTokenizedCorpusIdsByCorpusVersionIdQuery,
        RequestResult<IEnumerable<TokenizedCorpusId>>>
    {
        public Task<RequestResult<IEnumerable<TokenizedCorpusId>>>
            Handle(GetAllTokenizedCorpusIdsByCorpusVersionIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: query TokenizedCorpus table by CorpusVersion.Corpus and return enumerable.
            if (command.CorpusVersionId.Id.Equals(new System.Guid("ca761232ed4211cebacd00aa0057b223")))
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
