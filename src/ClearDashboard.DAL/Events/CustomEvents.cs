using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Events
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
