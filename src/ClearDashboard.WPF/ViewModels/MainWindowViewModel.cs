using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Helpers;

namespace ClearDashboard.Wpf.ViewModels
{
    public class MainWindowViewModel: AbstractModelBase
    {
        private string _version;

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged("");
            }
        }



        public MainWindowViewModel()
        {
            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Revision}.{thisVersion.Build}";
        }
    }
}
