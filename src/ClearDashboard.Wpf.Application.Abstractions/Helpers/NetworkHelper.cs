using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        private static extern bool InternetGetConnectedState(out int description, int reservedValue);

        public static bool IsInternetAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }
    }
}
