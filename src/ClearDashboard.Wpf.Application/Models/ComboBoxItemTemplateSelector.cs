using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Models
{
    /// <summary>
    /// Adapted from https://stackoverflow.com/questions/3995853/how-to-display-a-different-value-for-dropdown-list-values-selected-item-in-a-wpf
    /// </summary>
    public class ComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DropDownTemplate { get; set; }
        public DataTemplate SelectedTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ComboBoxItem comboBoxItem = GetVisualParent<ComboBoxItem>(container);
            if (comboBoxItem != null)
            {
                return DropDownTemplate;
            }
            return SelectedTemplate;
        }
        public static T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;
            while (child is not null and not T)
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }
    }
}
