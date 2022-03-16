using System;

namespace ClearDashboard.ParatextPlugin
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
    }

    public enum ActionType
    {
        Unknown,
        SendText,
    }
}
