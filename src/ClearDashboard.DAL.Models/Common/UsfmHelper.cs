using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class UsfmHelper
    {
        public string Path { get; set; } = string.Empty;
        public int NumberOfErrors { get; set; } = 0;
        public List<UsfmError> UsfmErrors { get; set; } = new();
    }

    public class UsfmError
    {
        public string Error { get; set; }
        public string Reference { get; set; }
    }
}
