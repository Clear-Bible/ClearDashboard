using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm
{
    public record GetBookUsfmByParatextIdBookIdQuery(string ParatextId, int BookNum) : IRequest<
        RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>>
    {
        public string ParatextId { get; } = ParatextId;
        public int BookNum { get; } = BookNum;
    }
}
