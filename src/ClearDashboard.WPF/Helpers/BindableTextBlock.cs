using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ClearDashboard.Wpf.Helpers
{
    public class BindableTextBlock : TextBlock
    {
        public ObservableCollection<Inline> InlineList
        {
            get
            {
                return (ObservableCollection<Inline>)GetValue(InlineListProperty);
            }
            set
            {
                SetValue(InlineListProperty, value);
            }
        }

        public new string FontFamily = "Segoe UI";
        public new float FontSize = 13;

        public static new readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(float), typeof(BindableTextBlock), new UIPropertyMetadata(13f, OnPropertyChanged));


        public static new readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(string), typeof(BindableTextBlock), new UIPropertyMetadata("Segoe UI", OnPropertyChanged));


        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Inline>), typeof(BindableTextBlock), new UIPropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BindableTextBlock textBlock = (BindableTextBlock)sender;
            textBlock.Inlines.Clear();
            if (e.NewValue != null)
            {
                textBlock.Inlines.AddRange((ObservableCollection<Inline>)e.NewValue);
            }
        }


    }
}
