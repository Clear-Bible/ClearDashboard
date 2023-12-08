using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class StringHelpers
    {
        public static string RemoveNonNumeric(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }
    }
}
