using System;

namespace NamedPipes
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

        ServerClosed,

        SendText,

        GetBibilicalTerms,
        GetSourceVerses,
        GetTargetVerses,
        GetChapter,
        GetNotes,

        SetBiblicalTerms,
        SetSourceVerseText,
        SetTargetVerseText,
        SetNotesObject,

        CurrentVerse,
    }

}
