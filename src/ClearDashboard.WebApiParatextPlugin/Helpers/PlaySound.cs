using System.Linq;
using System.Media;
using System.Reflection;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class PlaySound
    {
        public static void PlaySoundFromResource(SoundType soundType = SoundType.Success)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var audio = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(x => x.EndsWith(".wav"))
                .ToList();

            string soundFile = "";

            switch (soundType)
            {
                //case SoundType.Success:
                //    soundFile = "ClearDashboard.Wpf.Application.Resources.Crystal Click 2.wav";
                //    break;

                case SoundType.Error:
                    soundFile = "ClearDashboard.WebApiParatextPlugin.Resources.DashboardFailure.wav";
                    break;

                //case SoundType.Disconnected:
                //    soundFile = "ClearDashboard.Wpf.Application.Resources.plugin_disconnect.wav";
                //    break;
            }

            if (soundFile != "")
            {
                using var stream = assembly.GetManifestResourceStream(soundFile);
                {
                    var player = new SoundPlayer(stream);
                    player.Play();
                }
            }
        }
    }

    public enum SoundType
    {
        Success,
        Error,
        Disconnected
    }
}
