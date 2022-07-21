using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.ReferenceUsfm
{
    public record GetReferenceUsfmQuery(string Id) : IRequest<RequestResult<DataAccessLayer.Models.Common.ReferenceUsfm>>
    {
        public string Id { get; } = Id;
    }
}
