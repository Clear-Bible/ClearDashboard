using MediatR;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DataAccessLayer.Data
{
    static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, ProjectDbContext ctx, CancellationToken cancellationToken)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Models.IdentifiableEntity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents!)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}