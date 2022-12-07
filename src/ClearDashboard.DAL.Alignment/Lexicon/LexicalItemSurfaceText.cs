using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class LexicalItemSurfaceText
    {
        public LexicalItemSurfaceTextId? LexicalItemSurfaceTextId
        {
            get;
#if DEBUG
            internal set;
#else 
            // RELEASE MODIFIED
            //internal set;
            set;
#endif
        }
        public string? SurfaceText { get; set; }

        public LexicalItemSurfaceText()
        {
        }
        internal LexicalItemSurfaceText(LexicalItemSurfaceTextId lexicalItemSurfaceTextId, string surfaceText)
        {
            LexicalItemSurfaceTextId = lexicalItemSurfaceTextId;
            SurfaceText = surfaceText;
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (LexicalItemSurfaceTextId == null)
            {
                return;
            }

            var command = new DeleteLexicalItemSurfaceTextCommand(LexicalItemSurfaceTextId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }
    }
}
