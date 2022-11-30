using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;
using ClearBible.Engine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetParatextProjectIdForNoteIdQuery(NoteId NoteId) : ProjectRequestQuery<(Guid paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>;
}
