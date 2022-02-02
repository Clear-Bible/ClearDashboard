using ClearDashboard.Common.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace ClearDashboard.Wpf.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {

        private string _Version;
        [JsonProperty]
        public string Version
        {
            get => _Version;
            set { SetProperty(ref _Version, value); }
        }

        public MainWindowViewModel()
        {
            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Revision}.{thisVersion.Build}";
        }
    }
}
