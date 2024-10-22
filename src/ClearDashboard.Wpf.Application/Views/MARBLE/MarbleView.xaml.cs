﻿using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.Application.Views.Marble
{
    /// <summary>
    /// Interaction logic for MarbleView.xaml
    /// </summary>
    public partial class MarbleView : UserControl
    {
        public MarbleView()
        {
            InitializeComponent();
        }

        private void AllowScrolling_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Source is ListBox listbox && listbox == VersesListBox)
            {
                e.Handled = false;
            }
            else if (e.Source is ListBox relatedLemmas && relatedLemmas == RelatedLemmas && relatedLemmas.ActualHeight >= 300)
            {
                e.Handled = false;
            }
            else
            {
                MarbleScrollViewer.ScrollToVerticalOffset(MarbleScrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private void FindText_OnClick(object sender, RoutedEventArgs e)
        {
            DrawerHost.OpenDrawerCommand.Execute(Dock.Top, null);
        }

        private void Verses_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(VersesListBox);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta/40);
            e.Handled = true;
        }

        private void RelatedLemmas_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(RelatedLemmas);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 40);
            e.Handled = true;
        }
    }
}
