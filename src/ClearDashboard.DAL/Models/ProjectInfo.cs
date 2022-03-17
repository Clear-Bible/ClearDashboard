using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ProjectInfo
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public byte[] CreationDate { get; set; }
        public byte[] IsRtl { get; set; }
        public long? LastContentWordLevel { get; set; }
    }
}
