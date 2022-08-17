using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm
{

    public record GetRowsByParatextProjectIdAndBookIdQuery
        (string ParatextProjectId, string BookId) : IRequest<RequestResult<List<UsfmVerse>>>
    {
        public string ParatextProjectId { get; } = ParatextProjectId;
        public string BookId { get; } = BookId;
    }
}
