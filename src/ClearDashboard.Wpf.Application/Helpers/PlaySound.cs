using System;
using System.Linq;
using System.Media;
using System.Reflection;

namespace ClearDashboard.Wpf.Application.Helpers
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

            switch(soundType)
            {
                case SoundType.Success:
                { 
                    using var stream =
                        assembly.GetManifestResourceStream("ClearDashboard.Wpf.Application.Resources.Crystal Click 2.wav");
                    { 
                        var player = new SoundPlayer(stream);
                        player.Play();
                    }
                } 
                    break;

                case SoundType.Error:
                {
                    using var stream =
                        assembly.GetManifestResourceStream("ClearDashboard.Wpf.Application.Resources.DashboardFailure.wav");
                    {
                        var player = new SoundPlayer(stream);
                        player.Play();
                    }
                }
                    break;
            }
        }
    }

    public enum SoundType
    {
        Success, 
        Error
    }
}
