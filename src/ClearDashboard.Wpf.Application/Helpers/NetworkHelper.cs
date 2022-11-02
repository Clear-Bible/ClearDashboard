using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
