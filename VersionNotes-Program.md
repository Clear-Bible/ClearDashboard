# VersionNotes
## An application for creating Dashboard Release Notes

This application creates the JSON file used by Clear Dashboard with the Change Log and the version number for the Dashboard program to check against.  The JSON file is part of the **CLEAR_External_Releases** Git repository.  The file must be called `ClearDashboard.json` to be picked up by Dashboard.

### How to Create the Change Log

#### Step 1:
Create your Change Logs in the master Change Log file up on Google Docs: https://docs.google.com/document/d/1voRKJLLMWfBXWmgahvxV6hHLrIM6MCtBYD_5l889_PM/edit#

Add in a new line that starts with: `Version: `  Example:

```
Version: 0.4.0.0
```

Then add in a new bullet for each line of the change log using the following prefix tags:

- Added
- Updated
- Bug Fix
- Changed
- Deferred

followed by a note about the change.  Examples:

```
 * Added - On adding in a new Paratext Corpus, you can now search for the corpus name in the dropdown box.
 * Updated - Update the way that the EnhancedView’s listview control was handling the currently selected listviewitem which was impacting the notes handling.
 * Bug Fix - Unhandled threading error on NoteDisplay that caused the note to not update in the VerseDisplay control.
 * Changed - Biblical terms now loaded even when looking at it
 * Deferred - There are upcoming changes to Notes that are not included in this release.

```

#### Step 2:
1. Start the VersionNotes program
2. In the Google Document, highlight everything from the version number to the end of the version notes and copy it to the clipbloard.
3. Paste that content in the upper text box in VersionNotes.
4. Press the "Process RTB" button to parse the text and generate all the log lines in the panel at the bottom.

#### Step 3:
- If not detected automatically, change the version number and release date to the correct values.
- You can modify, delete, or add new log items from the bottom controls.

#### Step 4:
- Export the file into the Git project https://github.com/Clear-Bible/CLEAR_External_Releases as the filename `ClearDashboard.json` (case sensitive)
- Do a Git Push to send the changes back to GitHub.