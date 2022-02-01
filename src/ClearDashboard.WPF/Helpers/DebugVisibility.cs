using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Helpers
{
    /* Usage:

    xmlns:wpfUtil="clr-namespace:ClearSuite.Core.Helpers;assembly=WpfUtil"

    <Button Name="CmdTest" Click="CmdTestOnClick" Visibility="{x:Static wpfUtil:DebugVisibility.DebugOnly}">Test</Button>

    */
    public static class DebugVisibility
    {
        public static Visibility DebugOnly
        {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
        }

        public static Visibility ReleaseOnly
        {
#if DEBUG
            get { return Visibility.Collapsed; }
#else
            get { return Visibility.Visible; }
#endif
        }
    }
}
