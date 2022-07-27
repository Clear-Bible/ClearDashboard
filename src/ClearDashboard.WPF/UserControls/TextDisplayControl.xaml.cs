using System;
using System.Collections;
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
        /// <summary>
        /// Identifies the Words dependency property.
        /// </summary>
        public static readonly DependencyProperty WordsProperty = DependencyProperty.Register("Words", typeof(IEnumerable), typeof(TextDisplayControl));

        /// <summary>
        /// Gets or sets the words to display in the control.
        /// </summary>
        public IEnumerable Words
        {
            get => (IEnumerable)GetValue(WordsProperty);
            set => SetValue(WordsProperty, value);
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TextDisplayControl));

        /// <summary>
        /// Gets or sets the orientation for text display.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Identifies the InnerMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(TextDisplayControl), 
            new PropertyMetadata(new Thickness(6, 2, 6, 2)));

        /// <summary>
        /// Gets or sets the margin around each word for text display.
        /// </summary>
        public Thickness InnerMargin
        {
            get
            {

                var result = GetValue(InnerMarginProperty);
                return (Thickness) result;
            }
            set => SetValue(InnerMarginProperty, value);
        }

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
        
        /// <summary>
        /// Identifies the ParagraphMode dependency property.
        /// </summary>
        public static readonly DependencyProperty ParagraphModeProperty = DependencyProperty.Register("ParagraphMode", typeof(bool), typeof(TextDisplayControl));
        
        /// <summary>
        /// Gets or sets whether paragraph mode is enabled.
        /// </summary>
        public bool ParagraphMode
        {
            get => (bool)GetValue(ParagraphModeProperty);
            set => SetValue(ParagraphModeProperty, value);
        }

        public ItemsPanelTemplate ItemsPanelTemplate => (ItemsPanelTemplate) FindResource(Wrap || ParagraphMode ? "WrapPanelTemplate" : "StackPanelTemplate");

        public Brush WordBorderBrush => ParagraphMode ? Brushes.Transparent : (Brush) FindResource("MaterialDesignBody");

        private Thickness ParagraphMargin = new(0,0,0,0);
        private Thickness StackMargin = new(6,2,6,2);
        //public Thickness InnerMargin => ParagraphMode ? ParagraphMargin : StackMargin;        
        
        private Thickness ParagraphPadding = new(5,0,5,0);
        private Thickness StackPadding = new(10,2,10,2);
        public Thickness InnerPadding => ParagraphMode ? ParagraphPadding : StackPadding;
        
        public TextDisplayControl()
        {
            InitializeComponent();
            //DataContext = this;
        }
    }
}
