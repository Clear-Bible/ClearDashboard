
namespace ClearDashboard.Wpf.Models
{
    public class LayoutFile
    {
        public enum eLayoutType
        {
            Standard,
            Project
        }

        public string LayoutName { get; set; }
        public string LayoutPath { get; set; }
        public string LayoutID { get; set; }
        public eLayoutType LayoutType { get; set; } = eLayoutType.Project;
    }
}
