using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public record GetWordAnalysesQuery(string ProjectId) : IRequest<RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>>
    {
        public string ProjectId { get; } = ProjectId;
    }
}
