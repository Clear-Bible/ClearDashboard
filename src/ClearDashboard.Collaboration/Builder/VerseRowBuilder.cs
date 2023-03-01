using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.Collaboration.Factory;

namespace ClearDashboard.Collaboration.Builder;

public class VerseRowBuilder : GeneralModelBuilder<Models.VerseRow>
{
    public static GeneralListModel<GeneralModel<Models.VerseRow>> BuildModelSnapshots(Guid tokenizedCorpusId, BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.VerseRow>>();

        var verseRowModels = GetVerseRows(builderContext.ProjectDbContext, tokenizedCorpusId);
        foreach (var verseRowModel in verseRowModels)
        {
            modelSnapshots.Add(BuildModelSnapshot(verseRowModel, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.VerseRow> BuildModelSnapshot(Models.VerseRow verseRow, BuilderContext builderContext)
    {
        var modelProperties = ExtractUsingModelRefs(
            verseRow,
            builderContext,
            new List<string>() { "Id", "BookChapterVerse" });

        var modelSnapshot = new GeneralModel<Models.VerseRow>("BookChapterVerse", verseRow.BookChapterVerse!);
        AddPropertyValuesToGeneralModel(modelSnapshot, modelProperties);

        return modelSnapshot;
    }

    public static Dictionary<string, IEnumerable<Models.VerseRow>> GetVerseRowsByBook(ProjectDbContext projectDbContext, Guid tokenizedCorpusId)
    {
        return projectDbContext.VerseRows
            .Where(vr => vr.TokenizedCorpusId == tokenizedCorpusId)
            .ToList()
            .GroupBy(vr => vr.BookChapterVerse!.Substring(0, 3))
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(vr => vr).OrderBy(vr => vr.BookChapterVerse!).AsEnumerable());
    }

    public static IEnumerable<Models.VerseRow> GetVerseRows(ProjectDbContext projectDbContext, Guid tokenizedCorpusId)
    {
        return projectDbContext.VerseRows
            .Where(vr => vr.TokenizedCorpusId == tokenizedCorpusId)
            .OrderBy(vr => vr.BookChapterVerse)
            .ToList();
    }

    public static void SaveVerseRows(GeneralListModel<GeneralModel<Models.VerseRow>> verseRowSnapshots, string childPath, JsonSerializerOptions options)
    {
        var verseRowsByBook = verseRowSnapshots
            .GroupBy(vr => ((string)vr[nameof(Models.VerseRow.BookChapterVerse)]!).Substring(0, 3))
            .ToDictionary(g => g.Key, g => g
                .Select(vr => vr)
                .OrderBy(vr => ((string)vr[nameof(Models.VerseRow.BookChapterVerse)]!))
                .ToGeneralListModel<GeneralModel<Models.VerseRow>>())
            .OrderBy(kvp => kvp.Key);

        foreach (var verseRowsForBook in verseRowsByBook)
        {
            // Instead of the more general GeneralModelJsonConverter, this will use the
            // more specific VerseRowModelJsonConverter:
            var serializedChildModelSnapshot = JsonSerializer.Serialize<GeneralListModel<GeneralModel<Models.VerseRow>>>(
                verseRowsForBook.Value,
                options);
            File.WriteAllText(
                Path.Combine(
                    childPath,
                    string.Format(ProjectSnapshotFactoryCommon.VerseRowByBookFileNameTemplate, verseRowsForBook.Key)),
                serializedChildModelSnapshot);
        }
    }
}
