using System.Collections.Generic;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Application.Models;

/// <summary>
/// A record containing the metadata needed to send a note to external drafting tool.
/// </summary>
/// <remarks>
/// In order for a note to be sent to external drafting tool:
/// 
/// 1) All of the associated entities need to be tokens;
/// 2) All of the tokens must originate from the same external corpus;
/// 3) All of the tokens must be contiguous in the corpus.
///
/// This determination is made by ExternalNoteManager.NoteManager.GetNoteDetailsAsync
/// </remarks>
/// <param name="ExternalProjectId">The external project ID.</param>
/// <param name="TokenizedTextCorpusId">The <see cref="TokenizedTextCorpusId"/> for the related external corpus.</param>
/// <param name="VerseTokens">The tokens that make up the surrounding verse.</param>
public record ExternalSendNoteInformation(string ExternalProjectId, TokenizedTextCorpusId TokenizedTextCorpusId, IEnumerable<Token> VerseTokens);