using System;
using System.ComponentModel;
using System.Threading.Channels;
using ClearDashboard.DAL.Events;

namespace ClearDashboard.DAL
{
    public class StartUp
    {
        // event handler to be raised when the Paratext Username changes
        public static event EventHandler ParatextUserNameEventHandler;

        public StartUp()
        {
            // connect to paratext via named pipes


            GetParatextUserName();
        }

        public void GetParatextUserName()
        {
            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs("Dirk Kaiser"));
        }

    }
}
