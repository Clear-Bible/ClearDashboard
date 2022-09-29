using System;
using System.Linq;
using System.Media;
using System.Reflection;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class PlaySound
    {
        public static void PlaySoundFromResource(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var audio = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(x => x.EndsWith(".wav"))
                .ToList();


            System.IO.Stream s = assembly.GetManifestResourceStream("ClearDashboard.Wpf.Application.Resources.Crystal Click 2.wav");
            SoundPlayer player = new SoundPlayer(s);
            player.Play();
        }
    }
}
