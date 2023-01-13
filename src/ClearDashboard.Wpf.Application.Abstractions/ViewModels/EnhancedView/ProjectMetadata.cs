using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public static class ProjectMetadata
{
    public static ParatextProjectMetadata HebrewManuscriptMetadata => new ParatextProjectMetadata
    {
        Id = ManuscriptIds.HebrewManuscriptId,
        CorpusType = CorpusType.ManuscriptHebrew,
        Name = "Macula Hebrew",
        AvailableBooks = BookInfo.GenerateScriptureBookList(),
    };

    public static ParatextProjectMetadata GreekManuscriptMetadata => new ParatextProjectMetadata
    {
        Id = ManuscriptIds.GreekManuscriptId,
        CorpusType = CorpusType.ManuscriptGreek,
        Name = "Macula Greek",
        AvailableBooks = BookInfo.GenerateScriptureBookList(),
    };
}