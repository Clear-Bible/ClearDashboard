﻿using System;
using System.Collections.Generic;
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

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// TriggerProgBar logic for ColorStyles.xaml
    /// </summary>
    public partial class ColorStyles : Window
    {

        public ColorStyles()
        {
            InitializeComponent();
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            var ClickedButton = e.OriginalSource as Button;
            try
            {
                var brush = ClickedButton.Background;
                SolidColorBrush scb = brush as SolidColorBrush;
                Clipboard.Clear();
                Clipboard.SetText(String.Format("Name: {0}\nHex Code: {1}", ClickedButton.Content.ToString(), scb.Color.ToString()));
            }
            catch (Exception exception)
            {
                Clipboard.Clear();
                Clipboard.SetText(exception.Message);
            }

        }
    }
}
