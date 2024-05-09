using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class CreateNoteDomainEntityAssociationCommandHandler : ProjectDbContextCommandHandler<CreateNoteDomainEntityAssociationCommand,
        RequestResult<NoteDomainEntityAssociationId>, NoteDomainEntityAssociationId>
    {
        private readonly IMediator _mediator;

        public CreateNoteDomainEntityAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateNoteDomainEntityAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<NoteDomainEntityAssociationId>> SaveDataAsync(CreateNoteDomainEntityAssociationCommand request,
            CancellationToken cancellationToken)
        {
            string name;
            Guid guid;

            (name, guid) = GetNameAndId(request.DomainEntityId);

            var noteDomainEntityAssociation = ProjectDbContext!.NoteDomainEntityAssociations
                .FirstOrDefault(
                    nd => nd.NoteId == request.NoteId.Id && 
                    nd.DomainEntityIdGuid == guid
                );

            if (noteDomainEntityAssociation == null)
            {
                noteDomainEntityAssociation = new Models.NoteDomainEntityAssociation
                {
                    NoteId = request.NoteId.Id,
                    DomainEntityIdName = name,
                    DomainEntityIdGuid = guid
                };

                ProjectDbContext.NoteDomainEntityAssociations.Add(noteDomainEntityAssociation);
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }
            else if (name != noteDomainEntityAssociation.DomainEntityIdName)
            {
                noteDomainEntityAssociation.DomainEntityIdName = name;
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }

            return new RequestResult<NoteDomainEntityAssociationId>(new NoteDomainEntityAssociationId(noteDomainEntityAssociation.Id));
        }

        public static (string name, Guid id) GetNameAndId(IId iId)
        {
            Type? entityIdType = null;

            Type? baseType = iId.GetType().BaseType; //The issue is that replies do not have fully fledged Ids because their parents don't have fully fledged Ids

            if (baseType == null || baseType == typeof(System.Object))
            {
                baseType= iId.GetType();
            }
            
            while (baseType != null)
            {
                if (baseType.IsGenericType)
                {
                    var genericTypeDefinition = baseType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(EntityId<>))
                    {
                        entityIdType = genericTypeDefinition.MakeGenericType(baseType.GenericTypeArguments); //t.GetType()
                        break;
                    }

                }
                baseType = baseType.BaseType;
            }

            if (entityIdType == null)
                throw new InvalidParameterEngineException(name: "iId", value: iId.GetType().FullName ?? "GetType().FullName is null", message: "not a derivative of generic EntityId<>");

            return (entityIdType.AssemblyQualifiedName ?? throw new InvalidStateEngineException(name: "AssemblyQualifiedName", value: iId.GetType().FullName ?? "", message: "is null"), iId.Id);
        }
    }
}