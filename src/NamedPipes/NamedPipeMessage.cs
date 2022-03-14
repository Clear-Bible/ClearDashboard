using Newtonsoft.Json;
using System;

namespace NamedPipes
{
    [Serializable]
    public class NamedPipeMessage
    {
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
