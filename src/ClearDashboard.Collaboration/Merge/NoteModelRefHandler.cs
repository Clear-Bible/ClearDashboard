using System;
using System.Linq.Expressions;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Collaboration.Exceptions;
using System.Data.Common;

namespace ClearDashboard.Collaboration.Merge;

/// <summary>
/// NoteModelRef is essentially a NoteDomainEntityAssociation.
/// The ModelRef property of NoteModelRef has to be resolved in
/// two parts:
/// - the TokenRef(s)
/// - the containing ModelRef (TranslationRef or AlignmentRef)
/// Once the 'domain entity id' part of NoteModelRef is resolved,
/// it can be transformed and stored as a NoteDomainEntityAssociation
/// in the current database (assuming the referenced domain entity
/// ids can be found!)
/// </summary>
public class NoteModelRefHandler : DefaultMergeHandler<NoteModelRef>
{
    public delegate Guid ModelRefResolver<T>(T modelRef, ProjectDbContext projectDbContext, ILogger logger);

    public static readonly ModelRefResolver<TokenRef> tokenRefResolver =
        (TokenRef tokenRef, ProjectDbContext projectDbContext, ILogger logger) =>
            {
                var tokenComponentId = projectDbContext.TokenComponents
                    .Where(e => e.TokenizedCorpusId == tokenRef.TokenizedCorpusId!)
                    .Where(e => e.EngineTokenId == tokenRef.TokenLocation)
                    .Select(e => e.Id)
                    .FirstOrDefault();

                if (tokenComponentId == default)
                    throw new PropertyResolutionException($"{nameof(TokenRef)} having TokenizedCorpusId '{tokenRef.TokenizedCorpusId}' and TokenLocation '{tokenRef.TokenLocation}' cannot be resolved to a TokenComponent");

                return tokenComponentId;
            };

    private static readonly ModelRefResolver<TranslationRef> translationRefResolver =
        (TranslationRef translationRef, ProjectDbContext projectDbContext, ILogger logger) =>
            {
                var sourceTokenComponentId = tokenRefResolver.Invoke(translationRef.SourceTokenRef, projectDbContext, logger);

                var translationId = projectDbContext.Translations
                    .Where(e => e.TranslationSetId == translationRef.TranslationSetId)
                    .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
                    .Select(e => e.Id)
                    .FirstOrDefault();

                if (translationId == default)
                    throw new PropertyResolutionException($"{nameof(TranslationRef)} having TranslationSetId '{translationRef.TranslationSetId}' and SourceTokenComponentId '{sourceTokenComponentId}' cannot be resolved to a Translation");

                return translationId;
            };

    private static readonly ModelRefResolver<AlignmentRef> alignmentRefResolver =
        (AlignmentRef alignmentRef, ProjectDbContext projectDbContext, ILogger logger) =>
            {
                var sourceTokenComponentId = tokenRefResolver.Invoke(alignmentRef.SourceTokenRef, projectDbContext, logger);
//                var targetTokenComponentId = tokenRefResolver.Invoke(alignmentRef.TargetTokenRef, projectDbContext, logger);

                var alignmentId = projectDbContext.Alignments
                    .Where(e => e.AlignmentSetId == alignmentRef.AlignmentSetId)
                    .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
// To resolve the alignment 'ref' to an actual alignment, only use AlignmentSetId and SourceTokenComponentId, I think!
//                    .Where(e => e.TargetTokenComponentId == targetTokenComponentId)
                    .Select(e => e.Id)
                    .FirstOrDefault();

                if (alignmentId == default)
                    throw new PropertyResolutionException($"{nameof(AlignmentRef)} having AlignmentSetId '{alignmentRef.AlignmentSetId}' and SourceTokenComponentId '{sourceTokenComponentId}' cannot be resolved to an Alignment");

                return alignmentId;
            };

    private static readonly ModelRefResolver<DomainEntityRef> domainEntityRefResolver =
        (DomainEntityRef domainEntityRef, ProjectDbContext projectDbContext, ILogger logger) =>
            domainEntityRef.DomainEntityIdGuid;

    public NoteModelRefHandler(MergeContext mergeContext): base(mergeContext)
    {
        // These will allow MergeBehavior to convert our ModelRef property
        // to DomainEntityIdGuid and DomainEntityIdName, respectively.

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.NoteDomainEntityAssociation), nameof(Models.NoteDomainEntityAssociation.DomainEntityIdGuid)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not NoteModelRef)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of NoteModelRef");
                }

                var resolvedValue = ((NoteModelRef)modelSnapshot).ModelRef switch
                {
                    TokenRef modelRef => tokenRefResolver.Invoke((TokenRef)modelRef, projectDbContext, logger),
                    TranslationRef modelRef => translationRefResolver.Invoke((TranslationRef)modelRef, projectDbContext, logger),
                    AlignmentRef modelRef => alignmentRefResolver.Invoke((AlignmentRef)modelRef, projectDbContext, logger),
                    DomainEntityRef modelRef => domainEntityRefResolver.Invoke((DomainEntityRef)modelRef, projectDbContext, logger),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(NoteModelRef.ModelRef),
                        $"Not expected type value: {((NoteModelRef)modelSnapshot).ModelRef.GetType().ShortDisplayName()}")

                };

                await Task.CompletedTask;
                return resolvedValue;
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.NoteDomainEntityAssociation), nameof(Models.NoteDomainEntityAssociation.DomainEntityIdName)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not NoteModelRef)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of NoteModelRef");
                }

                var resolvedValue = ((NoteModelRef)modelSnapshot).ModelRef switch
                {
                    TokenRef modelRef => typeof(EntityId<TokenId>).AssemblyQualifiedName,
                    TranslationRef modelRef => typeof(EntityId<TranslationId>).AssemblyQualifiedName,
                    AlignmentRef modelRef => typeof(EntityId<AlignmentId>).AssemblyQualifiedName,
                    DomainEntityRef modelRef => modelRef.DomainEntityIdName.ToDomainEntityIdType().AssemblyQualifiedName,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(NoteModelRef.ModelRef),
                        $"Not expected type value: {((NoteModelRef)modelSnapshot).ModelRef.GetType().ShortDisplayName()}")
                };

                await Task.CompletedTask;
                return resolvedValue;
            });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.NoteDomainEntityAssociation), nameof(NoteModelRef.ModelRef)),
            new[] {
                nameof(Models.NoteDomainEntityAssociation.DomainEntityIdGuid),
                nameof(Models.NoteDomainEntityAssociation.DomainEntityIdName)
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.NoteDomainEntityAssociation), "NoteDomainEntityAssociationId"),
            new[] { nameof(Models.NoteDomainEntityAssociation.Id) });

        mergeContext.MergeBehavior.AddIdReversePropertyNameMapping(
            (typeof(Models.NoteDomainEntityAssociation), nameof(Models.NoteDomainEntityAssociation.Id)),
            "NoteDomainEntityAssociationId");
    }
}

