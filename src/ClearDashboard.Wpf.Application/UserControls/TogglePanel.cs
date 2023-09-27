using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A custom <see cref="Panel"/> implementation consisting of a header, an expand/collapse icon, and content that can be shown or hidden.
    /// </summary>
    public class TogglePanel : HeaderedContentControl
    {
        static TogglePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TogglePanel), new FrameworkPropertyMetadata(typeof(TogglePanel)));
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(TogglePanel));

        public bool IsOpen
        {
            get => (bool) GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }


        public override void OnApplyTemplate()
        {
            if (GetTemplateChild("HeaderButton") is Button headerButton)
            {
                headerButton.Click += TogglePanelOpen;
            }
            if (GetTemplateChild("ToggleButton") is Button iconButton)
            {
                iconButton.Click += TogglePanelOpen;
            }

            base.OnApplyTemplate();
        }

        private void TogglePanelOpen(object s, RoutedEventArgs e)
        {
            IsOpen = ! IsOpen;
        }
    }
}

