using System;

namespace Pipes_Shared
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

        GetBibilicalTerms,
        GetSourceVerses,
        GetTargetVerses,
        GetNotes,
        GetProject,
        GetCurrentVerse,

        SetBiblicalTerms,
        SetSourceVerseText,
        SetUSX,
        SetUSFM,
        SetNotesObject,

        CurrentVerse,
    }
}
