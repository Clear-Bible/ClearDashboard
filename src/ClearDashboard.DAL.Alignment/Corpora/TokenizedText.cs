using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    internal class TokenizedText : ScriptureText, ICache
    {
        protected readonly TokenizedTextCorpusId tokenizedCorpusId_;
        protected readonly IMediator mediator_;

        protected IEnumerable<TextRow>? TextRowsCache { get; set; }
        public bool UseCache { get; set; }
        public void InvalidateCache()
        {
            TextRowsCache = null;
        }

        public TokenizedText(TokenizedTextCorpusId tokenizedCorpusId, IMediator mediator, ScrVers versification, string bookId, bool useCache)
            : base(bookId, versification)
        {
            tokenizedCorpusId_ = tokenizedCorpusId;
            mediator_ = mediator;

            UseCache = useCache;
            TextRowsCache = null;
        }
        protected override IEnumerable<TextRow> GetVersesInDocOrder()
        {
            if (UseCache)
            {
                if (TextRowsCache == null)
                {
                    TextRowsCache = GetTextRows(mediator_, tokenizedCorpusId_, Id);
                }

                return TextRowsCache;
            }
            else
            {
                return GetTextRows(mediator_, tokenizedCorpusId_, Id);
            }
        }

        protected IEnumerable<TextRow> GetTextRows(IMediator mediator,TokenizedTextCorpusId tokenizedTextCorpusId, string bookId)
        {
            var command = new GetTokensByTokenizedCorpusIdAndBookIdQuery(tokenizedTextCorpusId, bookId); //Note that in ScriptureText Id is the book abbreviation bookId.

            var result = Task.Run(() => mediator.Send(command)).GetAwaiter().GetResult();
            result.ThrowIfCanceledOrFailed();

            var verses = result.Data;

            if (verses == null)
                throw new MediatorErrorEngineException("GetTokensByTokenizedCorpusIdAndBookIdQuery returned null data");

            return verses
                .SelectMany(verse => CreateRows(verse.Chapter, verse.Verse, "", verse.IsSentenceStart) // text parameter is set by TokensTextRow from the tokens
                    .Select(tr => new TokensTextRow(tr, verse.Tokens.ToList()))); //MUST return TokensTextRow. 
        }
    }
}
