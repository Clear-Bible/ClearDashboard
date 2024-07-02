using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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

        private void ProjectListView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                ScrollBar.LineDownCommand.Execute(null, e.OriginalSource as IInputElement);
            }
            else if (e.Delta > 0)
            {
                ScrollBar.LineUpCommand.Execute(null, e.OriginalSource as IInputElement);
            }
            e.Handled = true;
        }
    }
}
