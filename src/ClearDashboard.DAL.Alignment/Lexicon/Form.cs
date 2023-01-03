using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Form
    {
        public FormId? FormId
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
        public string? Text { get; set; }

        public Form()
        {
        }
        internal Form(FormId formId, string text)
        {
            FormId = formId;
            Text = text;
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (FormId == null)
            {
                return;
            }

            var command = new DeleteFormCommand(FormId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }
    }
}
