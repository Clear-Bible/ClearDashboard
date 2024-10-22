﻿using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using System;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Messages
{
    public record RefreshVerse(ReloadType ReloadType = ReloadType.Refresh);
    public record IsBackgroundDeletionTaskRunning(string TaskName, ParallelCorpusConnectorViewModel ConnectorDraggedOut, ParallelCorpusConnectorViewModel ConnectorDraggedOver, ParallelCorpusConnectionViewModel NewConnection);
    public record BackgroundDeletionTaskRunning(bool Result, ParallelCorpusConnectorViewModel ConnectorDraggedOut, ParallelCorpusConnectorViewModel ConnectorDraggedOver, ParallelCorpusConnectionViewModel NewConnection);

    public record RedrawParallelCorpusMenus();
    public record RedrawCorpusNodeMenus();

    public record GetExternalNotesMessage(string ParatextProjectId);
    public record SetIsCheckedAlignment(AlignmentSetId AlignmentSetId, bool IsChecked);

    public record SetProjectMetadataQuery(List<ParatextProjectMetadata> ProjectMetadata);

    public record GetApplicationWindowSettings();
    public record ApplicationWindowSettings(WindowSettings WindowSettings);

    public record UiLanguageChangedMessage(string LanguageCode);

    public record VerseChangedMessage(string Verse, bool OverrideParatextSync = false);
    public record BcvArrowMessage(BcvArrow Arrow);
    public record ProjectLoadCompleteMessage(bool Loaded);

    public record PluginClosingMessage(PluginClosing PluginClosing);
    public record ProjectChangedMessage(ParatextProject Project);

    public record TextCollectionChangedMessage(List<TextCollection> TextCollections);

    public record ParatextConnectedMessage(bool Connected);

    public record ReloadProjectPickerProjects();

    public record DeletedGitProject(Guid Guid);

    public record UserMessage(User User);
    public record DashboardProjectNameMessage(string ProjectName);

    public record DashboardProjectPermissionLevelMessage(PermissionLevel PermissionLevel);

    public record FilterPinsMessage(string Message, XmlSource XmlSource = XmlSource.All);

    public record CreateProjectMessage(string Message);

    public record ReloadExternalNotesDataMessage(ReloadType reloadType);

    public record ReloadProjectMessage();

    public record ReloadNotesListMessage();

    public record RefreshCheckGitLab();

    public record ProjectLoadedMessage();

    public record ProjectsMetadataChangedMessage(List<ParatextProjectMetadata> ProjectsMetadata);

    public record RefreshTextCollectionsMessage();

    public record RebuildMainMenuMessage();

    public record ParatextSyncMessage(bool Synced, object Parent);
}
