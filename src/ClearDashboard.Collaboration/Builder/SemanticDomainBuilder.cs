using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class SemanticDomainBuilder : GeneralModelBuilder<Models.Lexicon_SemanticDomain>
{
    public const string SEMANTIC_DOMAIN_MEANING_ASSOCIATIONS_CHILD_NAME = "SemanticDomainMeaningAssociations";

    public const string SEMANTIC_DOMAIN_REF_PREFIX = "SemanticDomain";
    public const string SEMANTIC_DOMAIN_MEANING_ASSOCIATION_REF_PREFIX = "SemanticDomainMeaning";

    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) }
        };

    public Func<ProjectDbContext, IEnumerable<Models.Lexicon_SemanticDomain>> GetSemanticDomains = (projectDbContext) =>
    {
        return projectDbContext.Lexicon_SemanticDomains
            .Include(e => e.SemanticDomainMeaningAssociations)
                .ThenInclude(e => e.Meaning)
                    .ThenInclude(e => e.Lexeme)
            .OrderBy(c => c.Created)
            .ToList();
    };

    public override IEnumerable<GeneralModel<Models.Lexicon_SemanticDomain>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.Lexicon_SemanticDomain>>();

        var modelItems = GetSemanticDomains(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public static GeneralModel<Models.Lexicon_SemanticDomain> BuildModelSnapshot(Models.Lexicon_SemanticDomain semanticDomain, BuilderContext builderContext)
    {
        var semanticDomainRef = CalculateSemanticDomainRef(semanticDomain.Text!);
        var modelSnapshot = BuildRefModelSnapshot(
                semanticDomain,
                semanticDomainRef,
                null,
                builderContext);


        if (semanticDomain.SemanticDomainMeaningAssociations.Any())
        {
            var modelSnapshotAssociations = new GeneralListModel<GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>>();
            foreach (var association in semanticDomain.SemanticDomainMeaningAssociations)
            {
                var meaningRef = LexiconBuilder.CalculateMeaningRef(association.Meaning!.Text!, association.Meaning!.Language!);
                var lexemeRef = LexiconBuilder.CalculateLexemeRef(association.Meaning!.Lexeme!.Lemma!, association.Meaning!.Lexeme!.Language!, association.Meaning!.Lexeme!.Type);

                var sdmaModelSnapshot = BuildRefModelSnapshot(
                    association,
                    CalculateSemanticDomainMeaningAssociationRef(
                        association.Meaning!.Lexeme!.Lemma!,
                        association.Meaning!.Lexeme!.Language!,
                        association.Meaning!.Lexeme!.Type,
                        association.Meaning!.Text!, 
                        association.Meaning!.Language!, 
                        semanticDomain.Text!),
                    new (string, string?, bool)[] {
                        (SEMANTIC_DOMAIN_REF_PREFIX, semanticDomainRef, true),
                        (LexiconBuilder.MEANING_REF_PREFIX, meaningRef, true),
                        (LexiconBuilder.LEXEME_REF_PREFIX, lexemeRef, false)
                    },
                    builderContext);

                modelSnapshotAssociations.Add(sdmaModelSnapshot);
            }

            modelSnapshot.AddChild(SEMANTIC_DOMAIN_MEANING_ASSOCIATIONS_CHILD_NAME, modelSnapshotAssociations.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    private static string CalculateSemanticDomainRef(string semanticDomainText)
    {
        return EncodePartsToRef(SEMANTIC_DOMAIN_REF_PREFIX, semanticDomainText);
    }

    public static string DecodeSemanticDomainRef(string semanticDomainRef)
    {
        var parts = DecodeRefToParts(SEMANTIC_DOMAIN_REF_PREFIX, semanticDomainRef, 1);
        return parts[0];
    }

    private static string CalculateSemanticDomainMeaningAssociationRef(string lexemeLemma, string lexemeLanguage, string? lexemeType, string meaningText, string meaningLanguage, string semanticDomainText)
    {
        return EncodePartsToRef(SEMANTIC_DOMAIN_MEANING_ASSOCIATION_REF_PREFIX, lexemeLemma, lexemeLanguage, lexemeType, meaningText, meaningLanguage, semanticDomainText);
    }

    public override GeneralModel<Models.Lexicon_SemanticDomain> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        // If the entity being deserialized has the older "Id" identity key,
        // convert it to the new system-neutral "Ref" key:
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            if (modelPropertiesTypes.TryGetValue(nameof(Models.Lexicon_SemanticDomain.Text), out var semanticDomainText))
            {
                modelPropertiesTypes.Remove(nameof(Models.Lexicon_SemanticDomain.Id));
                modelPropertiesTypes.Add(BuildPropertyRefName(), (typeof(string), CalculateSemanticDomainRef((string)semanticDomainText.value!)));
            }
            else
            {
                throw new PropertyResolutionException($"Semantic domain snapshot does not have Text property value, which is required for Ref calculation.");
            }
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }

    public override void UpdateModelSnapshotFormat(ProjectSnapshot projectSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings)
    {
        // For any semantic domain + meaning associations that have actual "Id"s, 
        // need to convert from:
        //      Id, SemanticDomainId, MeaningId
        // to:
        //      Ref, SemanticDomainRef, MeaningRef, LexemeRef
        // So...
        //  1.  Iterate though all associations and gather up Semantic Domain Ids and Meaning Ids that need to be converted
        //  2.  Iterate through lexicon data
        foreach (var semanticDomainSnapshot in projectSnapshot.GetGeneralModelList<Models.Lexicon_SemanticDomain>())
        {
            UpdateSemanticDomainMeaningAssociationChildren(semanticDomainSnapshot, updateMappings);
        }
    }

    private static void UpdateSemanticDomainMeaningAssociationChildren(GeneralModel<Models.Lexicon_SemanticDomain> parentSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings)
    {
        if (parentSnapshot.TryGetChildValue(SEMANTIC_DOMAIN_MEANING_ASSOCIATIONS_CHILD_NAME, out var children) &&
            children!.Any() &&
            children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>>)))
        {
            var childSnapshotsExisting = (IEnumerable<GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>>)children!;
            if (!childSnapshotsExisting.Any(e => e.IdentityKey == nameof(Models.Lexicon_SemanticDomainMeaningAssociation.Id)))
            {
                return;
            }

            if (!parentSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_SemanticDomain.Text), out var semanticDomainText))
            {
                throw new PropertyResolutionException($"Semantic domain snapshot does not have Text property value, which is required for creating SemanticDomainMeaningAssociation Ref values in FinalizeTopLevelEntities.");
            }

            var lexemeRefName = BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX);
            var meaningRefName = BuildPropertyRefName(LexiconBuilder.MEANING_REF_PREFIX);

            var modelSnapshotsNew = new GeneralListModel<GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>>();
            foreach (var modelSnapshotExisting in childSnapshotsExisting)
            {
                GeneralModel<Lexicon_SemanticDomainMeaningAssociation>? childSnapshotToAdd = null;

                if (modelSnapshotExisting.IdentityKey == nameof(Models.Lexicon_SemanticDomainMeaningAssociation.Id))
                {
                    if (!modelSnapshotExisting.TryGetGuidPropertyValue(nameof(Models.Lexicon_SemanticDomainMeaningAssociation.MeaningId), out var meaningId))
                    {
                        throw new PropertyResolutionException($"Lexicon_SemanticDomainMeaningAssociation snapshot does not have MeaningId property value, which is required when snapshot identity key is 'Id'.");
                    }

                    if (!updateMappings.TryGetValue(typeof(Models.Lexicon_Meaning), out var meaningMapping))
                    {
                        throw new PropertyResolutionException($"Update mappings is missing Lexicon_Meaning Type key.");
                    }

                    if (!meaningMapping.TryGetValue(meaningId, out var refMappings))
                    {
                        throw new PropertyResolutionException($"Update mappings does not contain meaningId '{meaningId}'.");
                    }

                    if (!refMappings.TryGetValue(lexemeRefName, out var lexemeRef) ||
                        !refMappings.TryGetValue(meaningRefName, out var meaningRef))
                    {
                        throw new PropertyResolutionException($"Update mappings for meaning '{meaningId}' does not contain both lexeme and meaning ref values.");
                    }

                    var modelPropertiesTypes = modelSnapshotExisting.ModelPropertiesTypes;

                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_SemanticDomainMeaningAssociation.Id));
                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_SemanticDomainMeaningAssociation.SemanticDomainId));
                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_SemanticDomainMeaningAssociation.MeaningId));

                    modelPropertiesTypes.Add(BuildPropertyRefName(SEMANTIC_DOMAIN_REF_PREFIX), (typeof(string), parentSnapshot.GetId()));
                    modelPropertiesTypes.Add(lexemeRefName, (typeof(string), lexemeRef));
                    modelPropertiesTypes.Add(meaningRefName, (typeof(string), meaningRef));

                    var (lexemeLemma, lexemeLanguage, lexemeType) = LexiconBuilder.DecodeLexemeRef(lexemeRef);
                    var (meaningText, meaningLanguage) = LexiconBuilder.DecodeMeaningRef(meaningRef);

                    childSnapshotToAdd = new GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>(
                        BuildPropertyRefName(),
                        CalculateSemanticDomainMeaningAssociationRef(lexemeLemma, lexemeLanguage, lexemeType, meaningText, meaningLanguage, semanticDomainText));

                    AddPropertyValuesToGeneralModel(childSnapshotToAdd, modelPropertiesTypes);
                }

                childSnapshotToAdd ??= modelSnapshotExisting;
                modelSnapshotsNew.Add(childSnapshotToAdd);
            }

            parentSnapshot.ReplaceChildrenForKey(SEMANTIC_DOMAIN_MEANING_ASSOCIATIONS_CHILD_NAME, modelSnapshotsNew.AsModelSnapshotChildrenList());
        }
    }
}
