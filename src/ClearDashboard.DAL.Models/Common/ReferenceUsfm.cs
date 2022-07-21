using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ReferenceUsfm
    {
        public string Name { get; set; } = "";
        public string LongName { get; set; } = "";
        public string Language { get; set; } = "";
        public string Id { get; set; } = "";
        public bool IsRTL { get; set; } = false;
        public string UsfmDirectoryPath { get; set; } = "";
    }
}
