using ClearApplicationFoundation.ViewModels.Infrastructure;
using HttpClientToCurl;
using System.Text.RegularExpressions;

namespace GenerateLicenseKeyForDashboard
{
    public class MainWindowViewModel: ApplicationScreen
    {
        private string _emailBox;
        public string EmailBox
        {
            get => _emailBox;
            set
            {
                _emailBox = value.Trim();
                NotifyOfPropertyChange(() => EmailBox);
            }
        }

        private string _firstNameBox;
        public string FirstNameBox
        {
            get => _firstNameBox;
            set
            {
                _firstNameBox = value.Trim();
                NotifyOfPropertyChange(() => FirstNameBox);
            }
        }

        private string _lastNameBox;
        public string LastNameBox
        {
            get => _lastNameBox;
            set
            {
                _lastNameBox = value.Trim();
                NotifyOfPropertyChange(() => LastNameBox);
            }
        }
    }
}
