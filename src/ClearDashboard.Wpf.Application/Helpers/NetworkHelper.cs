using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class NetworkHelper
    {
        public static async Task<bool> IsConnectedToInternet()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (await client.OpenReadTaskAsync(new Uri("http://clients3.google.com/generate_204", UriKind.Absolute)))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class InternetAvailability
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        public static bool IsInternetAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }
    }
}
