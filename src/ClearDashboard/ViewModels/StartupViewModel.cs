using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace ClearDashboard.Core.ViewModels
{
    public class StartupViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ILogger<StartupViewModel> _logger;
        public string VersionNum { get; set; }

        public IMvxCommand ResetTextCommand => new MvxCommand(ResetText);
        private void ResetText()
        {
            Text = "Hello MvvmCross";
        }

        private string _text = "Hello MvvmCross";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }



        public StartupViewModel(IMvxNavigationService navigationService, ILogger<StartupViewModel> logger)
        {
            //get the assembly version
            Version v = Assembly.GetEntryAssembly().GetName().Version;
            VersionNum = string.Format("Version: {0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);

            // wire up the viewmodel navigation
            _navigationService = navigationService;

            _logger = logger;

        }

    }
}
