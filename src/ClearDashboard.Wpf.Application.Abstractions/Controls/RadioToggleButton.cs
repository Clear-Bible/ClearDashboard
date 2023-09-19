using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Controls
{
    public class RadioToggleButton : RadioButton
    {
        protected override void OnToggle()
        {
            if (IsChecked == true) IsChecked = IsThreeState ? (bool?)null : (bool?)false;
            else IsChecked = IsChecked.HasValue;
        }
    }
}
