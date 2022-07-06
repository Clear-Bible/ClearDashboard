using System.Text;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetBookIdsByTokenizedCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetBookIdsByTokenizedCorpusIdQuery,
        RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>,
        (IEnumerable<string> bookId, CorpusId corpusId)>
    {

        public GetBookIdsByTokenizedCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetBookIdsByTokenizedCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>> GetDataAsync(GetBookIdsByTokenizedCorpusIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
            // pull out its parent CorpusId
            //Then iterate tokenization.Corpus(parent).Verses(child) and find unique bookAbbreviations and return as IEnumerable<string>

           
            // TODO: how to get the path here?
            var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, "FIX ME!");
            
            //var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath);

            return await Task.FromResult(
                new RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>
                (result: (corpus.Texts.Select(t => t.Id), new CorpusId(new Guid())),
                    success: true,
                    message: "successful result from test"));
        }

    }
}
