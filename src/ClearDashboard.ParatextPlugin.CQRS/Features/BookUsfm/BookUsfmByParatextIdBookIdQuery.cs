using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm
{
    public record GetBookUsfmByParatextIdBookIdQuery(string ParatextProjectId, int BookNum) : IRequest<RequestResult<List<UsfmVerse>>>
    {
        public string ParatextProjectId { get; } = ParatextProjectId;
        public int BookNum { get; } = BookNum;
    }
}
