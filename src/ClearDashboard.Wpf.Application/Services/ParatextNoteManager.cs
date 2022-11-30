using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;

namespace ClearDashboard.Wpf.Application.Services
{
    public class ParatextNoteManager
    {
        /// <summary>
        /// Gets the Paratext project ID for the entities associated with a note.
        /// </summary>
        /// <remarks>
        /// In order for a Paratext project ID to be returned:
        /// 1) All of the associated entities need to be tokens;
        /// 2) All of the tokens must originate from the same Paratext corpus;
        /// 3) All of the tokens must be contiguous in the corpus.
        /// </remarks>
        /// <param name="mediator"></param>
        /// <param name="noteId">The note for which to get the Paratext project ID.</param>
        /// <returns>The Paratext project ID if all three conditions are met; null otherwise.</returns>
        public static async Task<Guid?> GetParatextId(IMediator mediator, NoteId noteId)
        {
            var result = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(mediator, noteId);
            if (result is null)
            {
                return null;
            }

            (Guid paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens) = result.Value;
            // FIXME ANDY!
            // What do you want to do with 
            //    tokenizedTextCorpusId
            //    verseTokens

            return paratextId;
        }

        /// <summary>
        /// Sends a note to Paratext.
        /// </summary>
        /// <param name="note">The note to send to Paratext.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public static async Task SendToParatextAsync(NoteViewModel note)
        {
            // TODO: implement
            await Task.CompletedTask;
        }
    }
}
