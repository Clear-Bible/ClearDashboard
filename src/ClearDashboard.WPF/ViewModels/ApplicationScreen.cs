using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ApplicationScreen : Screen
    {
        protected ILog Logger { get; private set; }

        public ApplicationScreen()
        {

        }

        public ApplicationScreen(ILog logger)
        {
            Logger = logger;
        }
    }
}
