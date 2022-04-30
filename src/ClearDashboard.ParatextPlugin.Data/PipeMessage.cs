using System;

namespace ClearDashboard.ParatextPlugin.Data
{
    [Serializable]
    public class PipeMessage 
    {
        public PipeMessage()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public ActionType Action { get; set; }
        public string Text { get; set; } = string.Empty;

        public object Payload { get; set; } = null;
    }

    public enum ActionType
    {
        OnConnected,
        OnDisconnected,

        SendText,

        GetBibilicalTermsAll,
        GetBibilicalTermsProject,
        GetSourceVerses,
        GetTargetVerses,
        GetNotes,
        GetProject,
        GetCurrentVerse,
        GetUSX,
        GetUSFM,

        SetBiblicalTerms,
        SetSourceVerseText,
        SetUSX,
        SetUSFM,
        SetNotesObject,
        SetProject,

        CurrentVerse,
    }
}
