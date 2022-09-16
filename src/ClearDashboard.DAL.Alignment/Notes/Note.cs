using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class Note
    {
        private readonly IMediator mediator_;

        public NoteId? NoteId { get; private set; }
        public string Text { get; set; }
        public string? AbbreviatedText { get; set; }

        public Note(IMediator mediator, string text, string? abbreviatedText)
        {
            mediator_ = mediator;

            Text = text;
            AbbreviatedText = abbreviatedText;
        }
        internal Note(IMediator mediator, NoteId noteId, string text, string? abbreviatedText)
        {
            mediator_ = mediator;

            NoteId = noteId;
            Text = text;
            AbbreviatedText = abbreviatedText;
        }

        public async Task<Note> CreateOrUpdate(CancellationToken token = default)
        {            
            var command = new CreateOrUpdateNoteCommand(NoteId, Text, AbbreviatedText);

            var result = await mediator_.Send(command, token);
            if (result.Success)
            {
                NoteId = result.Data!;
                return this;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async void Delete(CancellationToken token = default)
        {
            if (NoteId == null)
            {
                return;
            }

            var command = new DeleteNoteAndAssociationsByNoteIdCommand(NoteId);

            var result = await mediator_.Send(command, token);
            if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<Note> Get(
            IMediator mediator,
            NoteId noteId)
        {
            var command = new GetNoteByNoteIdQuery(noteId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
