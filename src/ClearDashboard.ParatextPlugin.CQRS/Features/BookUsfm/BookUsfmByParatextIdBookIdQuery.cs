using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm
{
    public record GetBookUsfmByParatextIdBookIdQuery(string ParatextId, int BookNum) : IRequest<
        RequestResult<List<UsfmVerse>>>
    {
        public string ParatextId { get; } = ParatextId;
        public int BookNum { get; } = BookNum;
    }
}
