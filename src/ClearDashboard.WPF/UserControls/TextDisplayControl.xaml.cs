using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for TextDisplayControl.xaml
    /// </summary>
    public partial class TextDisplayControl : UserControl
    {
        public List<string> Words { get; set; } = new() { "alfa", "bravo", "charlie", "delta", "echo", "foxtrot", "golf", "hotel", "india", "juliet", "kilo", "lima", "mike" };

        public Orientation Orientation { get; set; } = Orientation.Horizontal;
        
        /// <summary>
        /// Identifies the Wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register("Wrap", typeof(bool), typeof(TextDisplayControl));
        
        /// <summary>
        /// Gets or sets whether the words should wrap in the control.
        /// </summary>
        public bool Wrap
        {
            get => (bool)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
        }

        public ItemsPanelTemplate ItemsPanelTemplate => (ItemsPanelTemplate) FindResource(Wrap ? "WrapPanelTemplate" : "StackPanelTemplate");

        public Brush WordBorderBrush => (Brush)FindResource("MaterialDesignBody");

        public TextDisplayControl()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
