using System.Diagnostics;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class JiraResultsViewModel : DashboardApplicationScreen
    {
        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        public JiraTicketResponse JiraTicketResponse = new();
        public JiraUser JiraUser = new();

        #endregion //Public Properties


        #region Observable Properties

        private string _link;
        public string Link
        {
            get => JiraTicketResponse.Self.ToString();
            set
            {
                _link = value;
                NotifyOfPropertyChange(() => Link);
            }
        }


        private string _userName;
        public string UserName
        {
            get => JiraUser.EmailAddress;
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
            }
        }


        private string _password;
        public string Password
        {
            get => JiraUser.Password;
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        #endregion //Constructor


        #region Methods


        public void ClickMarkDown()
        {
            Process.Start(Link);
        }


        #endregion // Methods



    }
}
