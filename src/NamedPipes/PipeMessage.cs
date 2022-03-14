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
        public NamedPipeMessage.ActionType Action { get; set; }
        public string Text { get; set; } = string.Empty;
    }

}
