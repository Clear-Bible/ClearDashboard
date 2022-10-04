using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization
{
    /// <summary>
    /// Represents a serialized enhanced view.
    /// </summary>
    public class SerializedEnhancedView
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool ParatextSync { get; set; }
        public string BBBCCCVVV { get; set; } = "001001001";
        public List<DisplayOrder> DisplayOrder { get; set; } = new List<DisplayOrder>();
    }
}
