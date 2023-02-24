using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Serializer;

namespace ClearDashboard.Collaboration.Builder;

public class VerseRowBuilder : GeneralModelBuilder<Models.VerseRow>
{
    //public static GeneralDictionaryModel<string, GeneralListModel<GeneralModel<Models.VerseRow>>> BuildSnapshotModel(Guid tokenizedCorpusId, BuilderContext builderContext)
    //{
    //    var verseRowsByBook = new GeneralDictionaryModel<string, GeneralListModel<GeneralModel<Models.VerseRow>>>();

    //    var verseRowModelsByBook = GetVerseRowsByBook(builderContext.ProjectDbContext, tokenizedCorpusId);
    //    foreach (var bookVerseRowModels in verseRowModelsByBook)
    //    {
    //        var verseRowsForBook = new GeneralListModel<GeneralModel<Models.VerseRow>>();
    //        foreach (var verseRow in bookVerseRowModels.Value)
    //        {
    //            verseRowsForBook.Add(BuildSnapshotModel(verseRow, builderContext));
    //        }

    //        verseRowsByBook.Add(bookVerseRowModels.Key, verseRowsForBook);
    //    }

    //    return verseRowsByBook;
    //}

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
        AddPropertyValuesToGenericModel(modelSnapshot, modelProperties);

        return modelSnapshot;
    }

    //public static void SerailizeToFilesystem(Dictionary<string, GeneralListModel<GeneralModel<Models.VerseRow>>> verseRowsByBook, string parentPath, JsonSerializerOptions jsonSerializerOptions)
    //{
    //    if (verseRowsByBook is not null && verseRowsByBook.Any())
    //    {
    //        var verseRowsByBookPath = Path.Combine(parentPath, "VerseRowsByBook");
    //        Directory.CreateDirectory(verseRowsByBookPath);

    //        foreach (var verseRowsForBook in verseRowsByBook)
    //        {
    //            // Instead of the more general GeneralModelJsonConverter, this will use the
    //            // more specific VerseRowModelJsonConverter:
    //            var serializedVerseRowsForBook = JsonSerializer.Serialize<GeneralListModel<GeneralModel<Models.VerseRow>>>(verseRowsForBook.Value, jsonSerializerOptions);
    //            File.WriteAllText(Path.Combine(verseRowsByBookPath, $"VerseRow_{verseRowsForBook.Key}"), serializedVerseRowsForBook);
    //        }
    //    }
    //}

    //public static Dictionary<string, GeneralListModel<GeneralModel<Models.VerseRow>>> DeseralizeFromFilesystem(string parentPath, JsonSerializerOptions jsonSerializerOptions)
    //{
    //    var verseRowsByBook = new Dictionary<string, GeneralListModel<GeneralModel<Models.VerseRow>>>();

    //    var verseRowsByBookPath = Path.Combine(parentPath, "VerseRowsByBook");
    //    if (Directory.Exists(verseRowsByBookPath))
    //    {
    //        foreach (string verseRowsForBookFile in Directory.GetFiles(verseRowsByBookPath).OrderBy(n => n))
    //        {
    //            var serializedVerseRowsForBook = File.ReadAllText(verseRowsForBookFile);
    //            var verseRowsForBook = JsonSerializer.Deserialize<GeneralListModel<GeneralModel<Models.VerseRow>>>(serializedVerseRowsForBook, jsonSerializerOptions);
    //            if (verseRowsForBook is null)
    //            {
    //                throw new ArgumentException($"Unable to deserialize verse row books properties at path {verseRowsForBookFile}");
    //            }

    //            var bookNumberAsString = verseRowsForBookFile.Substring(verseRowsForBookFile.LastIndexOf('_') + 1);
    //            verseRowsByBook.Add(bookNumberAsString, verseRowsForBook);
    //        }
    //    }

    //    return verseRowsByBook;
    //}

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
}
