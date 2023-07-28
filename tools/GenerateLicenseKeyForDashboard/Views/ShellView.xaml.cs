using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using GenerateLicenseKeyForDashboard.ViewModels;

namespace GenerateLicenseKeyForDashboard.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {

        private string _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private string _lastHeaderClickedDashboard = null;
        private ListSortDirection _lastDirectionDashboard = ListSortDirection.Ascending;

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
                var columnIndex = grid.CurrentColumn != null ? grid.CurrentColumn.DisplayIndex : 6;

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
                var columnIndex = grid.CurrentColumn != null ? grid.CurrentColumn.DisplayIndex : 6;

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

        private void CopyProjectUserConnection_OnClick(object sender, RoutedEventArgs e)
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
                var columnIndex = grid.CurrentColumn != null ? grid.CurrentColumn.DisplayIndex : 6;

                var cells = grid.SelectedCells;
                if (cells.Count != 0)
                {
                    var selectedItem = cells[0].Item;
                    var dashboardUser = (ProjectUserConnection)selectedItem;

                    switch (columnIndex)
                    {
                        case (0):
                            copyText = dashboardUser.UserName;
                            break;
                        case (1):
                            copyText = dashboardUser.ProjectName;
                            break;
                        case (2):
                            copyText = dashboardUser.AccessLevel.ToString();
                            break;
                    }
                }
            }

            Clipboard.SetText(copyText);
        }

        /// <summary>
        /// Handle the GitLab Users Grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var column = e.OriginalSource as DataGridColumnHeader;

            if (column == null)
            {
                return;
            }

            var textBlock = column.DataContext as TextBlock;

            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(GridGitLabUsers.ItemsSource);
            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                if (textBlock.Text == "GitLab Id")
                {
                    if (_lastHeaderClicked == textBlock.Text)
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            _lastDirection = ListSortDirection.Descending;
                        }
                        else
                        {
                            _lastDirection = ListSortDirection.Ascending;
                        }
                    }

                    cvTasks.SortDescriptions.Add(new SortDescription("Id", _lastDirection));
                }
                else
                {
                    if (_lastHeaderClicked == textBlock.Text)
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            _lastDirection = ListSortDirection.Descending;
                        }
                        else
                        {
                            _lastDirection = ListSortDirection.Ascending;
                        }
                    }


                    cvTasks.SortDescriptions.Add(new SortDescription(textBlock.Text, _lastDirection));
                }

            }

            _lastHeaderClicked = textBlock.Text;
        }

        /// <summary>
        /// Handle the Dashboard User's Grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DashboardGridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var column = e.OriginalSource as DataGridColumnHeader;

            if (column == null)
            {
                return;
            }

            var textBlock = column.DataContext as TextBlock;

            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(GridDashboardUsers.ItemsSource);
            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();

                if (_lastHeaderClickedDashboard == textBlock.Text)
                {
                    if (_lastDirectionDashboard == ListSortDirection.Ascending)
                    {
                        _lastDirectionDashboard = ListSortDirection.Descending;
                    }
                    else
                    {
                        _lastDirectionDashboard = ListSortDirection.Ascending;
                    }
                }

                cvTasks.SortDescriptions.Add(new SortDescription(textBlock.Text, _lastDirectionDashboard));
            }

            _lastHeaderClickedDashboard = textBlock.Text;
        }

        private void ConnectionGridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var column = e.OriginalSource as DataGridColumnHeader;

            if (column == null)
            {
                return;
            }

            var textBlock = column.DataContext as TextBlock;

            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(ProjectUserConnectionGrid.ItemsSource);
            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();

                if (_lastHeaderClickedDashboard == textBlock.Text)
                {
                    if (_lastDirectionDashboard == ListSortDirection.Ascending)
                    {
                        _lastDirectionDashboard = ListSortDirection.Descending;
                    }
                    else
                    {
                        _lastDirectionDashboard = ListSortDirection.Ascending;
                    }
                }

                cvTasks.SortDescriptions.Add(new SortDescription(textBlock.Text, _lastDirectionDashboard));
            }

            _lastHeaderClickedDashboard = textBlock.Text;
        }
    }
}
