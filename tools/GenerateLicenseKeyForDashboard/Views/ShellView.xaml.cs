using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GenerateLicenseKeyForDashboard.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }


        private void CopyGitUser_OnClick(object sender, RoutedEventArgs e)
        {
            string copyText = String.Empty;

            if (sender is MenuItem menuItem)
            {
                var parent = menuItem.Parent;
                var contextMenu = (ContextMenu)parent;
                sender = contextMenu.PlacementTarget;
            }

            if (sender is DataGrid grid)
            {
                var columnIndex = grid.CurrentColumn!=null ? grid.CurrentColumn.DisplayIndex : 6;

                var cells = grid.SelectedCells;
                if (cells.Count != 0)
                {
                    var selectedItem = cells[0].Item;
                    var gitUser = (GitUser)selectedItem;

                    switch (columnIndex)
                    {
                        case (0):
                            copyText = gitUser.Name;
                            break;
                        case (1):
                            copyText = gitUser.UserName;
                            break;
                        case (2):
                            copyText = gitUser.Email;
                            break;
                        case (3):
                            copyText = gitUser.Id.ToString();
                            break;
                        case (4):
                            copyText = gitUser.State;
                            break;
                        default:
                            copyText = gitUser.Email;
                            break;
                    }
                }
            }
            
            Clipboard.SetText(copyText);
        }

        private void CopyDashboardUser_OnClick(object sender, RoutedEventArgs e)
        {
            string copyText = String.Empty;

            if (sender is MenuItem menuItem)
            {
                var parent = menuItem.Parent;
                var contextMenu = (ContextMenu)parent;
                sender = contextMenu.PlacementTarget;
            }

            if (sender is DataGrid grid)
            {
                var columnIndex = grid.CurrentColumn!=null ? grid.CurrentColumn.DisplayIndex : 6;

                var cells = grid.SelectedCells;
                if (cells.Count != 0)
                {
                    var selectedItem = cells[0].Item;
                    var dashboardUser = (DashboardUser)selectedItem;

                    switch (columnIndex)
                    {
                        case (0):
                            copyText = dashboardUser.FirstName;
                            break;
                        case (1):
                            copyText = dashboardUser.LastName;
                            break;
                        case (2):
                            copyText = dashboardUser.Id.ToString();
                            break;
                        case (3):
                            copyText = dashboardUser.LicenseKey.ToString();
                            break;
                        case (4):
                            copyText = dashboardUser.ParatextUserName;
                            break;
                        case (5):
                            copyText = dashboardUser.Organization;
                            break;
                        case (6):
                            copyText = dashboardUser.IsInternal.ToString();
                            break;
                        case (7):
                            copyText = dashboardUser.Email;
                            break;
                        case (8):
                            copyText = dashboardUser.GitLabUserId.ToString();
                            break;
                        case (9):
                            copyText = dashboardUser.AppVersionNumber;
                            break;
                        case (10):
                            copyText = dashboardUser.AppLastDate.ToString();
                            break;
                    }
                }
            }
            
            Clipboard.SetText(copyText);
        }
    }
}
