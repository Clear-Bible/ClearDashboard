using System.Text;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
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


            // TODO: how to get the path here?
            var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, "FIX ME!");
            
            
            //var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath);
            
            return Task.FromResult(
                new RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>
                (result: (corpus.Texts.Select(t => t.Id), new CorpusId(new Guid())),
                success: true,
                message: "successful result from test"));
        }
    }
}
