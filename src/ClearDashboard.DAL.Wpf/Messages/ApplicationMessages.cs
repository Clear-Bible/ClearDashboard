using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Wpf.Messages
{

    public record SetProjectMetadataQuery(List<ParatextProjectMetadata> ProjectMetadata);

    public record GetApplicationWindowSettings();
    public record ApplicationWindowSettings(WindowSettings WindowSettings);


    public record UiLanguageChangedMessage(string LanguageCode);

    public record VerseChangedMessage(string Verse);
    public record ProjectLoadCompleteMessage(bool Loaded);


    public record ProjectChangedMessage(ParatextProject Project);

    public record TextCollectionChangedMessage(List<TextCollection> TextCollections);

    public record ParatextConnectedMessage(bool Connected);

    public record UserMessage(User User);


    public record FilterPinsMessage(string Message);

    public record CreateProjectMessage(string Message);

    public record ProjectsMetadataChangedMessage(List<ParatextProjectMetadata> ProjectsMetadata);
}
