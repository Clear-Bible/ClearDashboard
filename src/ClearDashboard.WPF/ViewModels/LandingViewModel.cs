using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Common.Models;
using MvvmHelpers;
using Serilog;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ObservableObject
    {
        public ILogger _logger { get; set; }

        public LandingViewModel()
        {
            // grab a copy of the current logger from the App.xaml.cs
            if (Application.Current is ClearDashboard.Wpf.App)
            {
                _logger = (Application.Current as ClearDashboard.Wpf.App)._logger;
            }
            
        }

    }
}
