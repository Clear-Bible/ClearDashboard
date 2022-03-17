using System;

namespace ClearDashboard.DataAccessLayer.Events
{
    public class CustomEvents
    {

        public class ParatextUsernameEventArgs : EventArgs
        {
            public string ParatextUserName { get; set; }

            public ParatextUsernameEventArgs(string paratextUserName)
            {
                this.ParatextUserName = paratextUserName;
            }
        }

    }
}
