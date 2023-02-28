using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Translation
    {
        public TranslationId? TranslationId
        {
            get;
#if DEBUG
            set;
#else 
            // RELEASE MODIFIED
            //internal set;
            set;
#endif
        }
        public string? Text { get; set; }

        public Translation()
        {
        }
        internal Translation(TranslationId translationId, string text)
        {
            TranslationId = translationId;
            Text = text;
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (TranslationId == null)
            {
                return;
            }

            var command = new DeleteTranslationCommand(TranslationId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }
    }
}
