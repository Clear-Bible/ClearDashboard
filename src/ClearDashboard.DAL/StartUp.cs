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
            // TODO connect to paratext via named pipes


            GetParatextUserName();
        }

        public void GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project's directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            Paratext.ParatextUtils paratextUtils = new Paratext.ParatextUtils();
            var user = paratextUtils.GetCurrentParatextUser();

            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs(user));
        }

    }
}
