using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NamedPipes
{
    public class NamedPipeMessage
    {
        public enum ActionType
        {
            OnConnected,
            OnDisconnected,

            GetBibilicalTerms,
            GetSourceVerses,
            GetTargetVerses,
            GetChapter,
            GetNotes,

            BiblicalTerms,
            ServerClosed,
            SourceVerseText,
            TargetVerseText,
            NotesObject,
            CurrentVerse,
        }

        [JsonProperty]
        public string actionType { get; set; }

        [JsonProperty]
        public string actionCommand;
        [JsonProperty]
        public string jsonPayload;

        public NamedPipeMessage(ActionType actionType, string actionCommand, string jsonPayload)
        {
            this.actionType = actionType.ToString();
            this.actionCommand = actionCommand;
            this.jsonPayload = jsonPayload;
        }

        public string CreateMessage()
        {
            // NamedPipeMessage namedPipeMsg = new NamedPipeMessage(actionType, actionCommand, jsonPayload);

            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
