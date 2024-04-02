using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Commands;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.CommandReceivers
{
	public class GetVerseRangeTokensCommandReceiver : IApiCommandReceiver<GetVerseRangeTokensCommand, (IEnumerable<TokensTextRow> Rows, int IndexOfVerse)>
	{
		private readonly ILogger _logger;
		private readonly IMediator _mediator;
		public GetVerseRangeTokensCommandReceiver(ILogger<GetVerseRangeTokensCommandReceiver> logger, IMediator mediator)
		{
			_logger = logger;
			_mediator = mediator;
		}

		public async Task<(IEnumerable<TokensTextRow> Rows, int IndexOfVerse)> RequestAsync(GetVerseRangeTokensCommand command, CancellationToken cancellationToken)
		{
			var tokenizedCorpusId = new TokenizedTextCorpusId(command.TokenizedCorpusId);
			var tokenizedCorpus = await TokenizedTextCorpus.Get(_mediator, tokenizedCorpusId, true, cancellationToken);

			if (tokenizedCorpus == null)
			{
				throw new ArgumentException($"No TokenizedTextCorpus found for id '{command.TokenizedCorpusId}'");
			}

			var verseRange = tokenizedCorpus.GetByVerseRange(
				command.VerseRef, 
				command.NumberOfVersesInChapterBefore, 
				command.NumberOfVersesInChapterAfter);

			var tokensTextRows = verseRange.textRows.Cast<TokensTextRow>().ToList();
			return (tokensTextRows, verseRange.indexOfVerse);
		}
	}
}
