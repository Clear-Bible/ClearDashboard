using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.Application.Views.Project
{
    /// <summary>
    /// Interaction logic for EnhancedCorpusView.xaml
    /// </summary>
    public partial class EnhancedView : UserControl
    {
        public EnhancedView()
        {
            InitializeComponent();
        }

        private void InnerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var innerListView = sender as ListView;

            ItemContainerGenerator generator = innerListView.ItemContainerGenerator;
            ListBoxItem selectedItem = (ListBoxItem)generator.ContainerFromIndex(innerListView.SelectedIndex);
            VerseDisplay verseDisplay = GetChildrenByType(selectedItem, typeof(VerseDisplay), "VerseDisplay") as VerseDisplay;
            if (verseDisplay is not null)
            {
                if (this.DataContext is EnhancedViewModel)
                {
                    var vm = (EnhancedViewModel)this.DataContext;
                    vm.SelectedVerseDisplay = verseDisplay;
                }
            }
        }

        public Visual GetChildrenByType(Visual visualElement, Type typeElement, string nameElement)
        {
            if (visualElement == null) return null;
            if (visualElement.GetType() == typeElement)
            {
                FrameworkElement fe = visualElement as FrameworkElement;
                if (fe != null)
                {
                    if (fe.Name == nameElement)
                    {
                        return fe;
                    }
                }
            }
            Visual foundElement = null;
            if (visualElement is FrameworkElement)
                (visualElement as FrameworkElement).ApplyTemplate();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visualElement); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(visualElement, i) as Visual;
                foundElement = GetChildrenByType(visual, typeElement, nameElement);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }
    }
}
