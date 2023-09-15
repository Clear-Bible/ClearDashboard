using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LexiconBuilder : GeneralModelBuilder<Models.Lexicon_Lexeme>
{
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
        var modelSnapshot = ExtractUsingModelIds(lexeme, builderContext.CommonIgnoreProperties);

        if (lexeme.Meanings.Any())
        {
            var modelSnapshotMeanings = new GeneralListModel<GeneralModel<Models.Lexicon_Meaning>>();
            foreach (var meaning in lexeme.Meanings)
            {
                var lexiconMeaning = ExtractUsingModelIds<Models.Lexicon_Meaning>(meaning, builderContext.CommonIgnoreProperties);

                if (meaning.Translations.Any())
                {
                    var modelSnapshotTranslations = new GeneralListModel<GeneralModel<Models.Lexicon_Translation>>();
                    foreach (var translation in meaning.Translations)
                    {
                        modelSnapshotTranslations.Add(ExtractUsingModelIds<Models.Lexicon_Translation>(translation, builderContext.CommonIgnoreProperties));
                    }

                    lexiconMeaning.AddChild("Translations", modelSnapshotTranslations.AsModelSnapshotChildrenList());
                }

                modelSnapshotMeanings.Add(lexiconMeaning);
            }

            modelSnapshot.AddChild("Meanings", modelSnapshotMeanings.AsModelSnapshotChildrenList());
        }

        if (lexeme.Forms.Any())
        {
            var modelSnapshotForms = new GeneralListModel<GeneralModel<Models.Lexicon_Form>>();
            foreach (var form in lexeme.Forms)
            {
                modelSnapshotForms.Add(ExtractUsingModelIds<Models.Lexicon_Form>(form, builderContext.CommonIgnoreProperties));
            }

            modelSnapshot.AddChild("Forms", modelSnapshotForms.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }
}
