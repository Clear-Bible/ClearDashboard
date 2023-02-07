using ClearDashboard.DataAccessLayer;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class BindableTextBlock : TextBlock
    {
        public ObservableCollection<Inline> InlineList
        {
            get => (ObservableCollection<Inline>)GetValue(InlineListProperty);
            set => SetValue(InlineListProperty, value);
        }

        private static ObservableCollection<Inline> _oldValue;
        
        public new string FontFamily = FontNames.DefaultFontFamily;
        public new float FontSize = 13;

        public new static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(float), typeof(BindableTextBlock), new UIPropertyMetadata(13f, OnPropertyChanged));
        
        public new static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(string), typeof(BindableTextBlock), new UIPropertyMetadata(FontNames.DefaultFontFamily, OnPropertyChanged));
        
        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Inline>), typeof(BindableTextBlock), new UIPropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = (BindableTextBlock)sender;
            textBlock.Inlines.Clear();
            if (e.NewValue != null  && (ObservableCollection<Inline>)e.NewValue != _oldValue)
            {
                textBlock.Inlines.AddRange((ObservableCollection<Inline>)e.NewValue);
            }
            _oldValue = (ObservableCollection<Inline>)e.NewValue;
        }


    }
}
