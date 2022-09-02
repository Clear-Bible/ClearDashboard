namespace ClearDashboard.Wpf.Application.Models
{
    public class AlignmentPlan
    {
        public enum SMT
        {
            FastAlign,
            IBM4,
            IBM1,
            HMM,
        }

        public string Target { get; set; } = "";
        public string TargetID { get; set; } = "";
        public string Source { get; set; } = "";
        public string SourceID { get; set; } = "";
        public bool IsCleanUsfmComplete { get; set; } = false;
        public double CleanUsfmProgress { get; set; } = 0;
        public bool IsAlignmentComplete { get; set; } = false;
        public double AlignmentProgress { get; set; } = 0;
        public SMT AlignmentSMT { get; set; } = SMT.FastAlign;
    }
}
