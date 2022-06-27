using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetBookIdsByTokenizedCorpusIdQueryHandler : IRequestHandler<
        GetBookIdsByTokenizedCorpusIdQuery,
        RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>>
    {
        public Task<RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>>
            Handle(GetBookIdsByTokenizedCorpusIdQuery command, CancellationToken cancellationToken)
        {

            //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
            // pull out its parent CorpusId
            //Then iterate tokenization.Corpus(parent).Verses(child) and find unique bookAbbreviations and return as IEnumerable<string>

            var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath);
            
            return Task.FromResult(
                new RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>
                (result: (corpus.Texts.Select(t => t.Id), new CorpusId(new Guid())),
                success: true,
                message: "successful result from test"));
        }
    }
}
