using System;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Controls.DonutGraph.PieChart
{
    /// <summary>
    /// Defines the layout of the pie chart
    /// </summary>
    public partial class PieChartLayout : UserControl
    {
        #region dependency properties

        /// <summary>
        /// The property of the bound object that will be plotted (CLR wrapper)
        /// </summary>
        public String ManuscriptTextProperty
        {
            get
            {
                return GetManuscriptTextProperty(this);
            }
            set
            {
                SetManuscriptTextProperty(this, value);
            }
        }

        public static readonly DependencyProperty ManuscriptTextPropertyProperty =
            DependencyProperty.RegisterAttached("ManuscriptTextProperty", typeof(String), typeof(PieChartLayout),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));

        public static void SetManuscriptTextProperty(UIElement element, String value)
        {
            element.SetValue(ManuscriptTextPropertyProperty, value);
        }
        public static String GetManuscriptTextProperty(UIElement element)
        {
            return (String)element.GetValue(ManuscriptTextPropertyProperty);
        }



        /// <summary>
        /// The property of the bound object that will be plotted (CLR wrapper)
        /// </summary>
        public String PlottedProperty
        {
            get { return GetPlottedProperty(this); }
            set { SetPlottedProperty(this, value); }
        }

        // PlottedProperty dependency property
        public static readonly DependencyProperty PlottedPropertyProperty =
            DependencyProperty.RegisterAttached("PlottedProperty", typeof(String), typeof(PieChartLayout),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));

        // PlottedProperty attached property accessors
        public static void SetPlottedProperty(UIElement element, String value)
        {
            element.SetValue(PlottedPropertyProperty, value);
        }
        public static String GetPlottedProperty(UIElement element)
        {
            return (String)element.GetValue(PlottedPropertyProperty);
        }

        /// <summary>
        /// The property of the bound object that will be plotted (CLR wrapper)
        /// </summary>
        public String TitleProperty
        {
            get { return GetTitleProperty(this); }
            set { SetTitleProperty(this, value); }
        }

        // TitleProperty dependency property
        public static readonly DependencyProperty TitlePropertyProperty =
            DependencyProperty.RegisterAttached("TitleProperty", typeof(String), typeof(PieChartLayout),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));

        // TitleProperty attached property accessors
        public static void SetTitleProperty(UIElement element, String value)
        {
            element.SetValue(TitlePropertyProperty, value);
        }
        public static String GetTitleProperty(UIElement element)
        {
            return (String)element.GetValue(TitlePropertyProperty);
        }

        /// <summary>
        /// A class which selects a color based on the item being rendered.
        /// </summary>
        public IColorSelector ColorSelector
        {
            get { return GetColorSelector(this); }
            set { SetColorSelector(this, value); }
        }

        // ColorSelector dependency property
        public static readonly DependencyProperty ColorSelectorProperty =
            DependencyProperty.RegisterAttached("ColorSelectorProperty", typeof(IColorSelector), typeof(PieChartLayout),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        // ColorSelector attached property accessors
        public static void SetColorSelector(UIElement element, IColorSelector value)
        {
            element.SetValue(ColorSelectorProperty, value);
        }
        public static IColorSelector GetColorSelector(UIElement element)
        {
            return (IColorSelector)element.GetValue(ColorSelectorProperty);
        }


        #endregion

        public PieChartLayout()
        {
            InitializeComponent();
        }
    }
}
