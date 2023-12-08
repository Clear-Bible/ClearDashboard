using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record UpdateLabelsWithExternalLabelsCommand(UpdateLabelsWithExternalLabelsCommandParam Data) : ProjectRequestCommand<bool>
    {
        public UpdateLabelsWithExternalLabelsCommandParam Data { get; } = Data;
    }
}
