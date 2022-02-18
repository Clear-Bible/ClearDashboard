using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClearDashboard.Common.Models;

namespace ClearDashboard.Wpf.Helpers
{
    public class NavButton : ButtonBase
    {
        #region Dependency Properties

        public static readonly DependencyProperty ImageSourceProperty = 
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(NavButton), new PropertyMetadata(null));
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set
            {
                SetValue(ImageSourceProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NavButton), new PropertyMetadata(null));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public static readonly DependencyProperty NavUriProperty =
            DependencyProperty.Register("NavUri", typeof(Uri), typeof(NavButton), new PropertyMetadata(null));
        public Uri NavUri
        {
            get { return (Uri)GetValue(NavUriProperty); }
            set { SetValue(NavUriProperty, value); }
        }

        public static readonly DependencyProperty DashboardProjectProperty =
            DependencyProperty.Register("DashboardProject", typeof(DashboardProject), typeof(NavButton), new PropertyMetadata(null));
        public DashboardProject DashboardProject
        {
            get { return (DashboardProject)GetValue(DashboardProjectProperty); }
            set { SetValue(DashboardProjectProperty, value); }
        }

        #endregion


        #region Startup

        static NavButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavButton), new FrameworkPropertyMetadata(typeof(NavButton)));
        }

        #endregion
    }
}
