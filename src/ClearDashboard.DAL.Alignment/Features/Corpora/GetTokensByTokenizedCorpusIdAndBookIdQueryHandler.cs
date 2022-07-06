using System.Text;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
    {
        // **********************************************
        // TODO:  Remove the following two lines
        public static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "Corpora", "data");
        public static readonly string UsfmTestProjectPath = Path.Combine(TestDataPath, "usfm", "Tes");
        // **********************************************

        public GetTokensByTokenizedCorpusIdAndBookIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTokensByTokenizedCorpusIdAndBookIdQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider,logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>> GetDataAsync(GetTokensByTokenizedCorpusIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
            //Then iterate tokens and package them by verse then return enumerable.

            // TODO:  How do we get the Usfm Path?
            var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, UsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();

            //var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
            //    .Tokenize<LatinWordTokenizer>()
            //    .Transform<IntoTokensTextRowProcessor>();

            var chapterVerseTokens = corpus.GetRows(new List<string>() { request.BookId })
                .GroupBy(r => ((VerseRef)r.Ref).ChapterNum)
                .OrderBy(g => g.Key)
                .SelectMany(g => g
                    .GroupBy(sg => ((VerseRef)sg.Ref).VerseNum)
                    .OrderBy(sg => sg.Key)
                    .Select(sg => new
                    {
                        Chapter = g.Key,
                        Verse = sg.Key,
                        Tokens = sg
                            .SelectMany(v => ((TokensTextRow)v).Tokens),
                        IsSentenceStart = sg
                            .First().IsSentenceStart
                    })
                )
                .Select(cvts => (cvts.Chapter.ToString(), cvts.Verse.ToString(), cvts.Tokens, cvts.IsSentenceStart));


            return Task.FromResult(
                new RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
                (result: chapterVerseTokens,
                    success: true,
                    message: "successful result from test"));
        }
        
    }
}
