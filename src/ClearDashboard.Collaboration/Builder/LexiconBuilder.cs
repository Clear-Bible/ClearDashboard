using System.Text.RegularExpressions;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LexiconBuilder : GeneralModelBuilder<Models.Lexicon_Lexeme>
{
    public const string MEANINGS_CHILD_NAME = "Meanings";
    public const string TRANSLATIONS_CHILD_NAME = "Translations";
    public const string FORMS_CHILD_NAME = "Forms";

    public const string LEXEME_REF_PREFIX = "Lexeme";
    public const string MEANING_REF_PREFIX = "Meaning";
    public const string FORM_REF_PREFIX = "Form";
    public const string TRANSLATION_REF_PREFIX = "Translation";

    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) }
        };

    public Func<ProjectDbContext, IEnumerable<Models.Lexicon_Lexeme>> GetLexemes = (projectDbContext) =>
    {
        return projectDbContext.Lexicon_Lexemes
            .Include(e => e.Meanings)
                .ThenInclude(e => e.Translations)
            .Include(e => e.Forms)
            .OrderBy(c => c.Created)
            .ToList();
    };

    public override IEnumerable<GeneralModel<Models.Lexicon_Lexeme>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.Lexicon_Lexeme>>();

        var modelItems = GetLexemes(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public static GeneralModel<Models.Lexicon_Lexeme> BuildModelSnapshot(Models.Lexicon_Lexeme lexeme, BuilderContext builderContext)
    {
        var lexemeRef = CalculateLexemeRef(lexeme.Lemma!, lexeme.Language!, lexeme.Type);
        var modelSnapshot = BuildRefModelSnapshot(lexeme, lexemeRef, null, builderContext);

        if (lexeme.Meanings.Any())
        {
            var modelSnapshotMeanings = new GeneralListModel<GeneralModel<Models.Lexicon_Meaning>>();
            foreach (var meaning in lexeme.Meanings)
            {
                var meaningRef = CalculateMeaningRef(meaning.Text!, meaning.Language!);
                var lexiconMeaning = BuildRefModelSnapshot(
                    meaning, 
                    meaningRef, 
                    new (string, string?, bool)[] { (LEXEME_REF_PREFIX, lexemeRef, true) }, 
                    builderContext);

                if (meaning.Translations.Any())
                {
                    var modelSnapshotTranslations = new GeneralListModel<GeneralModel<Models.Lexicon_Translation>>();
                    foreach (var translation in meaning.Translations)
                    {
                        modelSnapshotTranslations.Add(BuildRefModelSnapshot(
                            translation,
                            CalculateTranslationRef(translation.Text!),
                            new (string, string?, bool)[] { (LEXEME_REF_PREFIX, lexemeRef, false), (MEANING_REF_PREFIX, meaningRef, true) },
                            builderContext));
                    }

                    lexiconMeaning.AddChild(TRANSLATIONS_CHILD_NAME, modelSnapshotTranslations.AsModelSnapshotChildrenList());
                }

                modelSnapshotMeanings.Add(lexiconMeaning);
            }

            modelSnapshot.AddChild(MEANINGS_CHILD_NAME, modelSnapshotMeanings.AsModelSnapshotChildrenList());
        }

        if (lexeme.Forms.Any())
        {
            var modelSnapshotForms = new GeneralListModel<GeneralModel<Models.Lexicon_Form>>();
            foreach (var form in lexeme.Forms)
            {
                modelSnapshotForms.Add(BuildRefModelSnapshot(
                    form, 
                    CalculateFormRef(form.Text!),
                    new (string, string?, bool)[] { (LEXEME_REF_PREFIX, lexemeRef, true) },
                    builderContext));
            }
            modelSnapshot.AddChild(FORMS_CHILD_NAME, modelSnapshotForms.AsModelSnapshotChildrenList());
        }
        return modelSnapshot;
    }

    private static string CalculateLexemeRef(string lemma, string language, string? type)
    {
        return EncodePartsToRef(LEXEME_REF_PREFIX, lemma, language, type);
    }

    public static (string lemma, string language, string? type) DecodeLexemeRef(string lexemeRef)
    {
        var parts = DecodeRefToParts(LEXEME_REF_PREFIX, lexemeRef, 3);
        return (lemma: parts[0], language: parts[1], type: string.IsNullOrEmpty(parts[2]) ? null : parts[2]);
    }

    private static string CalculateMeaningRef(string text, string language)
    {
        return EncodePartsToRef(MEANING_REF_PREFIX, text, language);
    }

    public static (string text, string language) DecodeMeaningRef(string meaningRef)
    {
        var parts = DecodeRefToParts(MEANING_REF_PREFIX, meaningRef, 2);
        return (text: parts[0], language: parts[1]);
    }

    private static string CalculateTranslationRef(string text)
    {
        return EncodePartsToRef(TRANSLATION_REF_PREFIX, text);
    }

    private static string CalculateFormRef(string text)
    {
        return EncodePartsToRef(FORM_REF_PREFIX, text);
    }

    public override GeneralModel<Models.Lexicon_Lexeme> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        // If the entity being deserialized has the older "Id" identity key,
        // convert it to the new system-neutral "Ref" key:
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            if (modelPropertiesTypes.TryGetValue(nameof(Models.Lexicon_Lexeme.Lemma), out var lemma) &&
                modelPropertiesTypes.TryGetValue(nameof(Models.Lexicon_Lexeme.Language), out var language))
            {
                modelPropertiesTypes.TryGetValue(nameof(Models.Lexicon_Lexeme.Type), out var type);

                modelPropertiesTypes.Remove(nameof(Models.Lexicon_Lexeme.Id));
                modelPropertiesTypes.Add(BuildPropertyRefName(), (typeof(string), CalculateLexemeRef((string)lemma.value!, (string)language.value!, (string?)type.value)));
            }
            else
            {
                throw new PropertyResolutionException($"Label snapshot does not have Text property value, which is required for Ref calculation.");
            }
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }

    public override void FinalizeTopLevelEntities(List<GeneralModel<Models.Lexicon_Lexeme>> topLevelEntities)
    {
        foreach (var topLevelEntity in topLevelEntities)
        {
            UpdateMeaningChildren(topLevelEntity);
            UpdateFormChildren(topLevelEntity);
        }
    }

    private static void UpdateMeaningChildren(GeneralModel<Lexicon_Lexeme> parentSnapshot)
    {
        if (parentSnapshot.TryGetChildValue(MEANINGS_CHILD_NAME, out var children) &&
            children!.Any() &&
            children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.Lexicon_Meaning>>)))
        {
            var childSnapshotsExisting = (IEnumerable<GeneralModel<Models.Lexicon_Meaning>>)children!;
            if (!childSnapshotsExisting.Any(e => e.IdentityKey == nameof(Models.Lexicon_Meaning.Id)))
            {
                // If the serialized Meaning was already the "Ref" form, all we need to do
                // here is propagate the Lexeme Ref value so that the handler
                // can find the exact meaning for update or delete:

                //foreach (var childSnapshot in childSnapshotsExisting)
                //{
                //    childSnapshot.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                //    UpdateTranslationChildren(childSnapshot, (string)parentSnapshot.GetId());
                //}
                return;
            }

            var modelSnapshotsNew = new GeneralListModel<GeneralModel<Models.Lexicon_Meaning>>();
            foreach (var modelSnapshotExisting in childSnapshotsExisting)
            {
                GeneralModel<Lexicon_Meaning>? childSnapshotToAdd = null;

                if (modelSnapshotExisting.IdentityKey == nameof(Models.Lexicon_Meaning.Id))
                {
                    if (!modelSnapshotExisting.TryGetStringPropertyValue(nameof(Models.Lexicon_Meaning.Text), out var meaningText) ||
                        !modelSnapshotExisting.TryGetStringPropertyValue(nameof(Models.Lexicon_Meaning.Language), out var meaningLanguage))
                    {
                        throw new PropertyResolutionException($"Lexicon_Meaning snapshot does not have both Text and Language property values, which are required for creating MeaningRef values in FinalizeTopLevelEntities.");
                    }

                    var modelPropertiesTypes = modelSnapshotExisting.ModelPropertiesTypes;

                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_Meaning.Id));
                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_Meaning.LexemeId));

                    childSnapshotToAdd = new GeneralModel<Models.Lexicon_Meaning>(
                        BuildPropertyRefName(),
                        CalculateMeaningRef(meaningText, meaningLanguage));

                    AddPropertyValuesToGeneralModel(childSnapshotToAdd, modelPropertiesTypes);
                    childSnapshotToAdd.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                    childSnapshotToAdd.CloneAllChildren(modelSnapshotExisting);
                }

                childSnapshotToAdd ??= modelSnapshotExisting;

                UpdateTranslationChildren(childSnapshotToAdd, (string)parentSnapshot.GetId());

                modelSnapshotsNew.Add(childSnapshotToAdd);
            }

            parentSnapshot.ReplaceChildrenForKey(MEANINGS_CHILD_NAME, modelSnapshotsNew.AsModelSnapshotChildrenList());
        }
    }

    private static void UpdateFormChildren(GeneralModel<Lexicon_Lexeme> parentSnapshot)
    {
        if (parentSnapshot.TryGetChildValue(FORMS_CHILD_NAME, out var children) &&
            children!.Any() &&
            children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.Lexicon_Form>>)))
        {
            var childSnapshotsExisting = (IEnumerable<GeneralModel<Models.Lexicon_Form>>)children!;
            if (!childSnapshotsExisting.Any(e => e.IdentityKey == nameof(Models.Lexicon_Form.Id)))
            {
                // If the serialized form was already the "Ref" form, all we need to do
                // here is propagate the Lexeme Ref value so that the handler
                // can find the exact form for update or delete:

                //foreach (var childSnapshot in childSnapshotsExisting)
                //{
                //    childSnapshot.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                //}

                return;
            }

            // If the serialized form was the older "Id" form, replace with the newer form:

            var modelSnapshotsNew = new GeneralListModel<GeneralModel<Models.Lexicon_Form>>();
            foreach (var modelSnapshotExisting in childSnapshotsExisting)
            {
                GeneralModel<Lexicon_Form>? childSnapshotToAdd = null;

                if (modelSnapshotExisting.IdentityKey == nameof(Models.Lexicon_Form.Id))
                {
                    if (!modelSnapshotExisting.TryGetStringPropertyValue(nameof(Models.Lexicon_Form.Text), out var formText))
                    {
                        throw new PropertyResolutionException($"Lexicon_Form snapshot does not have Text property value, which is required for creating TranslationRef values in FinalizeTopLevelEntities.");
                    }

                    var modelPropertiesTypes = modelSnapshotExisting.ModelPropertiesTypes;

                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_Form.Id));
                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_Form.LexemeId));

                    childSnapshotToAdd = new GeneralModel<Models.Lexicon_Form>(
                        BuildPropertyRefName(),
                        CalculateFormRef(formText));

                    AddPropertyValuesToGeneralModel(childSnapshotToAdd, modelPropertiesTypes);
                    childSnapshotToAdd.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                }

                childSnapshotToAdd ??= modelSnapshotExisting;
                modelSnapshotsNew.Add(childSnapshotToAdd);
            }

            parentSnapshot.ReplaceChildrenForKey(FORMS_CHILD_NAME, modelSnapshotsNew.AsModelSnapshotChildrenList());
        }
    }

    private static void UpdateTranslationChildren(GeneralModel<Lexicon_Meaning> parentSnapshot, string lexemeRef)
    {
        if (parentSnapshot.TryGetChildValue(TRANSLATIONS_CHILD_NAME, out var children) &&
            children!.Any() &&
            children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.Lexicon_Translation>>)))
        {
            var childSnapshotsExisting = (IEnumerable<GeneralModel<Models.Lexicon_Translation>>)children!;
            if (!childSnapshotsExisting.Any(e => e.IdentityKey == nameof(Models.Lexicon_Translation.Id)))
            {
                // If the serialized form was already the "Ref" form, all we need to do
                // here is propagate the Lexeme and Meaning Ref values so that the handler
                // can find the exact translation for update or delete:

                //foreach (var childSnapshot in childSnapshotsExisting)
                //{
                //    childSnapshot.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), lexemeRef, typeof(string));
                //    childSnapshot.Add(BuildPropertyRefName(MEANING_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                //}

                return;
            }

            // If the serialized form was the older "Id" form, replace with the newer form:

            var modelSnapshotsNew = new GeneralListModel<GeneralModel<Models.Lexicon_Translation>>();
            foreach (var modelSnapshotExisting in childSnapshotsExisting)
            {
                GeneralModel<Lexicon_Translation>? childSnapshotToAdd = null;

                if (modelSnapshotExisting.IdentityKey == nameof(Models.Lexicon_Translation.Id))
                {
                    if (!modelSnapshotExisting.TryGetStringPropertyValue(nameof(Models.Lexicon_Translation.Text), out var translationText))
                    {
                        throw new PropertyResolutionException($"Lexicon_Translation snapshot does not have Text property value, which is required for creating TranslationRef values in FinalizeTopLevelEntities.");
                    }

                    var modelPropertiesTypes = modelSnapshotExisting.ModelPropertiesTypes;

                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_Translation.Id));
                    modelPropertiesTypes.Remove(nameof(Models.Lexicon_Translation.MeaningId));

                    childSnapshotToAdd = new GeneralModel<Models.Lexicon_Translation>(
                        BuildPropertyRefName(),
                        CalculateTranslationRef(translationText));

                    AddPropertyValuesToGeneralModel(childSnapshotToAdd, modelPropertiesTypes);
                    childSnapshotToAdd.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), lexemeRef, typeof(string));
                    childSnapshotToAdd.Add(BuildPropertyRefName(MEANING_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                }

                childSnapshotToAdd ??= modelSnapshotExisting;
                modelSnapshotsNew.Add(childSnapshotToAdd);
            }

            parentSnapshot.ReplaceChildrenForKey(TRANSLATIONS_CHILD_NAME, modelSnapshotsNew.AsModelSnapshotChildrenList());
        }
    }
}
