using System.Net.NetworkInformation;

namespace ClearDashboard.Collaboration.Util;

public class Pinger
{
    public static bool PingHost(string nameOrAddress)
    {
        bool pingable = false;
        Ping pinger = null;

        try
        {
            pinger = new Ping();
            PingReply reply = pinger.Send(nameOrAddress);
            pingable = reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
        finally
        {
            pinger?.Dispose();
        }

        return pingable;
    }
}

