namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization
{
    public class EnhancedViewItemMetadatum
    {
        public MessageType MessageType { get; set; }
        public object? Data { get; set; }
    }

    public enum MessageType
    {
        ShowTokenizationWindowMessage,
        ShowParallelTranslationWindowMessage
    }
}
