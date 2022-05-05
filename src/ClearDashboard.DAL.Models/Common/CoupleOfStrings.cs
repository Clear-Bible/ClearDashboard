using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class CoupleOfStrings
    {
        public string stringA { get; set; } = "";
        public string stringB { get; set; } = "";
    }

    public class CoupleOfLists
    {
        public List<string> stringA { get; set; }
        public List<string> stringB { get; set; }
    }
}
