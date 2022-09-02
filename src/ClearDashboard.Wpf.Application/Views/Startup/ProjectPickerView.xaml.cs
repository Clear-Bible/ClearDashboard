using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ClearDashboard.DataAccessLayer.Models;
using Path = System.Windows.Shapes.Path;

namespace ClearDashboard.Wpf.Application.Views.Startup
{
    /// <summary>
    /// Interaction logic for ProjectPickerView.xaml
    /// </summary>
    public partial class ProjectPickerView : UserControl
    {
        public ProjectPickerView()
        {
            InitializeComponent();
        }

        //public void DeleteProject(DashboardProject project)
        //{
        //    if (!project.HasFullFilePath)
        //    {
        //        return;
        //    }
        //    var fi = new FileInfo(project.FullFilePath ?? throw new InvalidOperationException("Project full file path is null."));

        //    try
        //    {
        //        var di = new DirectoryInfo(fi.DirectoryName ?? throw new InvalidOperationException("File directory name is null."));

        //        foreach (var file in di.GetFiles())
        //        {
        //            file.Delete();
        //        }
        //        foreach (var dir in di.GetDirectories())
        //        {
        //            dir.Delete(true);
        //        }

        //        di.Delete();

        //        DashboardProjects.Remove(project);

        //        var originalDatabaseCopyName = $"{project.ProjectName}_original.sqlite";
        //        File.Delete(System.IO.Path.Combine(di.Parent.ToString(), originalDatabaseCopyName));
        //    }
        //    catch (Exception e)
        //    {
        //        Logger?.LogError(e, "An unexpected error occurred while deleting a project.");
        //    }
        //}

        ///// <summary>
        ///// Event raised to delete the project files.
        ///// </summary>
        //private void OnDeleteProject(object sender, ExecutedRoutedEventArgs e)
        //{
        //    DeleteProject(sender as DashboardProject);
        //}
        private void MoveForwards_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.LightBlue;
            }
        }

        private void MoveForwards_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.LightGray;
            }
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel element)
            {
                element.Background = Brushes.LightBlue;
            }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel element)
            {
                element.Background = Brushes.White;
            }
        }
    }
}
