using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    internal class TokenizedTextLocal : ScriptureText, ICache
    {
        protected readonly TokenizedTextCorpusId tokenizedCorpusId_;
        protected readonly IMediator mediator_;

        protected List<TextRow>? TextRowsCache { get; set; }
        public bool UseCache { get; set; }
        public bool NonTokenized { get; }

        public void InvalidateCache()
        {
            TextRowsCache = null;
        }

        public TokenizedTextLocal(
            TokenizedTextCorpusId tokenizedCorpusId,
			IMediator mediator, 
            ScrVers versification, 
            string bookId, 
            bool useCache,
            bool nonTokenized)
            : base(bookId, versification)
        {
            tokenizedCorpusId_ = tokenizedCorpusId;
            mediator_ = mediator;

            UseCache = useCache;
            NonTokenized = nonTokenized;
            TextRowsCache = null;
        }
        protected override IEnumerable<TextRow> GetVersesInDocOrder()
        {
            if (UseCache)
            {
                if (TextRowsCache == null)
                {
                    TextRowsCache = GetTextRows(mediator_, tokenizedCorpusId_, Id)
                        .ToList();
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
                .Select(CreateTokenTextRow);
        }

		protected TextRow CreateTokenTextRow(VerseTokens verseTokens)
        { 
            var verseRef = new VerseRef(Id, verseTokens.Chapter, verseTokens.Verse, Versification);

            var textRow = new TextRow(verseRef)
            {
                IsSentenceStart = verseTokens.IsSentenceStart,
                IsInRange = verseTokens.IsInRange,
                IsRangeStart = verseTokens.IsRangeStart,
                IsEmpty = verseTokens.IsEmpty
            };

            TokensTextRow tokensTextRow;
            if (NonTokenized)
            {
                var token = new Token(
                    new TokenId(verseRef.BookNum, verseRef.ChapterNum, verseRef.VerseNum, 1, 1), 
                        verseTokens.OriginalText, 
                        verseTokens.OriginalText
                    );
                tokensTextRow = new TokensTextRow(textRow, new List<Token>() { token });
            }
            else
            {
                tokensTextRow = new TokensTextRow(textRow, verseTokens.Tokens.ToList());
            }
            tokensTextRow.OriginalText = verseTokens.OriginalText;
            return tokensTextRow;
        }
    }
}
