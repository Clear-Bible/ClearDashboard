using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public static class ProjectMetadata
{
    public static ParatextProjectMetadata HebrewManuscriptMetadata => new ParatextProjectMetadata
    {
        Id = ManuscriptIds.HebrewManuscriptId,
        CorpusType = CorpusType.ManuscriptHebrew,
        Name = MaculaCorporaNames.HebrewCorpusName,
        AvailableBooks = BookInfo.GenerateScriptureBookList(),
    };

    public static ParatextProjectMetadata GreekManuscriptMetadata => new ParatextProjectMetadata
    {
        Id = ManuscriptIds.GreekManuscriptId,
        CorpusType = CorpusType.ManuscriptGreek,
        Name = MaculaCorporaNames.GreekCorpusName,
        AvailableBooks = BookInfo.GenerateScriptureBookList(),
    };
}