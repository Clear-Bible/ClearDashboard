using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record DeleteSemanticDomainSenseAssociationCommand(
        SemanticDomainId SemanticDomainId,
        SenseId SenseId) : ProjectRequestCommand<Unit>;
}
