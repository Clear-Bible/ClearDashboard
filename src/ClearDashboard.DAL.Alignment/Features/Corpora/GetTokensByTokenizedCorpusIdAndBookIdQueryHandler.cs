using System.Text;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : AlignmentDbContextQueryHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
    {


        public GetTokensByTokenizedCorpusIdAndBookIdQueryHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, ILogger logger) : base(projectNameDbContextFactory, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>> GetData(GetTokensByTokenizedCorpusIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
            //Then iterate tokens and package them by verse then return enumerable.

            // TODO:  How do we get the Usfm Path?
            var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, "<FIX ME>")
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
