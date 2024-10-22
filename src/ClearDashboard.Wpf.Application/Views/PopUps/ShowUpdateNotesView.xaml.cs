﻿using System;
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
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.Application.Views.PopUps
{
    /// <summary>
    /// Interaction logic for ShowUpdateNotesView.xaml
    /// </summary>
    public partial class ShowUpdateNotesView : Window
    {
        public ShowUpdateNotesView()
        {
            InitializeComponent();
        }

        private void NotesList_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(UpdateList);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta/3);
            e.Handled = true;
        }
    }
}
