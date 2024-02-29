using Autofac;
using ClearBible.Engine.Corpora;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Commands
{
	public class GetVerseRangeTokensCommand : IApiCommand<(IEnumerable<TokensTextRow> Rows, int IndexOfVerse)>
	{
		public required Guid TokenizedCorpusId { get; init; }
		public required VerseRef VerseRef { get; init; }
		public ushort NumberOfVersesInChapterBefore { get; init; } = 0;
		public ushort NumberOfVersesInChapterAfter { get; init; } = 0;

		public Task<(IEnumerable<TokensTextRow> Rows, int IndexOfVerse)> ExecuteAsync(IComponentContext context, CancellationToken cancellationToken)
		{
			var receiver = context.Resolve<IApiCommandReceiver<GetVerseRangeTokensCommand, (IEnumerable<TokensTextRow> Rows, int IndexOfVerse)>>();
			return Task.Run(() => receiver.RequestAsync(this, cancellationToken));
		}
	}
}
