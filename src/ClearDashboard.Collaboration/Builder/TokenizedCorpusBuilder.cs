using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Serializer;

namespace ClearDashboard.Collaboration.Builder;

public class TokenizedCorpusBuilder : GeneralModelBuilder<Models.TokenizedCorpus>
{
    public static IEnumerable<GeneralModel<Models.TokenizedCorpus>> BuildModelSnapshot(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.TokenizedCorpus>>();

        var modelItems = GetTokenizedCorpora(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }


    public static GeneralModel<Models.TokenizedCorpus> BuildModelSnapshot(Models.TokenizedCorpus tokenizedCorpus, BuilderContext builderContext)
    {
        //builderContext.IncrementModelNameIndexValue(nameof(Models.TokenizedCorpus));

        var modelSnapshot = ExtractUsingModelIds(tokenizedCorpus, builderContext.CommonIgnoreProperties);

        if (tokenizedCorpus.Corpus!.CorpusType != Models.CorpusType.ManuscriptHebrew &&
            tokenizedCorpus.Corpus!.CorpusType != Models.CorpusType.ManuscriptGreek)
        {
            modelSnapshot.Add("BookNumbers", GetBookNumbers(builderContext.ProjectDbContext, tokenizedCorpus.Id));

            //tokenizedCorpusExternalData.AddChild("VerseRowsByBook", GetVerseRowFieldsByBook(builderContext.ProjectDbContext, tokenizedCorpus.Id));
            modelSnapshot.AddChild("VerseRows", VerseRowBuilder.BuildModelSnapshots(tokenizedCorpus.Id, builderContext).AsModelSnapshotChildrenList());
        }

        var tokenCompositeTokens = TokenBuilder.GetTokenizedCorpusCompositeTokens(builderContext.ProjectDbContext, tokenizedCorpus.Id);
        if (tokenCompositeTokens.Any())
        {
            var compositeModelSnapshots = new GeneralListModel<GeneralModel<Models.TokenComposite>>();

            foreach (var tct in tokenCompositeTokens)
            {
                compositeModelSnapshots.Add(TokenBuilder.BuildModelSnapshot(tct.TokenComposite, tct.Tokens, builderContext));
            }
            
            modelSnapshot.AddChild("CompositeTokens", compositeModelSnapshots.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    public static IEnumerable<Models.TokenizedCorpus> GetTokenizedCorpora(ProjectDbContext projectDbContext)
    {
        return projectDbContext.TokenizedCorpora.Include(tc => tc.Corpus).OrderBy(c => c.Created).ToList();
    }

    public static GeneralListModel<string> GetBookNumbers(ProjectDbContext projectDbContext, Guid tokenizedCorpusId)
    {
        return projectDbContext.VerseRows
            .Where(vr => vr.TokenizedCorpusId == tokenizedCorpusId)
            .Select(vr => vr.BookChapterVerse!.Substring(0, 3))
            .Distinct()
            .OrderBy(b => b)
            .ToGeneralListModel();
    }

    public static Dictionary<string, Dictionary<string, string>> GetVerseRowFieldsByBook(ProjectDbContext projectDbContext, Guid tokenizedCorpusId)
    {
        return projectDbContext.VerseRows
            .Where(vr => vr.TokenizedCorpusId == tokenizedCorpusId)
            .Select(vr => new { bcv = vr.BookChapterVerse!, originalText = vr.OriginalText ?? "", vr.IsSentenceStart, vr.IsInRange, vr.IsRangeStart, vr.IsEmpty })
            .ToList()
            .GroupBy(bcvo => bcvo.bcv.Substring(0, 3))
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(bcvo => bcvo.bcv, bcvo => bcvo.originalText + ", " + bcvo.IsSentenceStart + ", " + bcvo.IsInRange + ", " + bcvo.IsRangeStart + ", " + bcvo.IsEmpty));
    }
}
