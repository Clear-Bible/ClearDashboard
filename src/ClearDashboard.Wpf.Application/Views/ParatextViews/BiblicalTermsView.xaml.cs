using System;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SIL.IO.FileLock;
using ClearDashboard.DAL.ViewModels;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for BiblicalTermsView.xaml
    /// </summary>
    public partial class BiblicalTermsView : UserControl
    {
        private BiblicalTermsViewModel _vm;
        public BiblicalTermsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = (BiblicalTermsViewModel)DataContext;

            // listen for changes to the lower listview to make it scroll back 
            // to the top
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("SelectedItemVerses"))
                {
                    if (SelectedItemVerses.Items.Count > 0)
                    {
                        SelectedItemVerses.ScrollIntoView(SelectedItemVerses.Items[0]);
                    }

                    return;
                    // execute code here.
                }
                if (args.PropertyName.Equals("WindowFlowDirection"))
                {
                    this.FlowDirection = _vm.WindowFlowDirection;
                }

            };
        }

        private void CopyText_OnClick(object sender, RoutedEventArgs e)
        {
            string copyText=String.Empty;

            var menuItem = (MenuItem)sender;
            var parent = menuItem.Parent;
            var contextMenu = (ContextMenu)parent;
            var target = contextMenu.PlacementTarget;

            if (target is DataGrid grid)
            {
                var columnIndex = grid.CurrentColumn!=null? grid.CurrentColumn.DisplayIndex : 6;

                var cells = grid.SelectedCells;
                var selectedItem = cells[0].Item;
                var BiblicalTermsItem = (BiblicalTermsData)selectedItem;

                switch (columnIndex)
                {
                    case (1):
                        copyText = BiblicalTermsItem.Id;
                        break;
                    case (2):
                        copyText = BiblicalTermsItem.SemanticDomain;
                        break;
                    case (3):
                        copyText = BiblicalTermsItem.Gloss;
                        break;
                    case (4):
                        copyText = BiblicalTermsItem.RenderingCount.ToString();
                        break;
                    case (5):
                        copyText = BiblicalTermsItem.References.Count.ToString();
                        break;
                    case (6):
                        copyText = BiblicalTermsItem.RenderingString;
                        break;
                    default:
                        copyText = BiblicalTermsItem.Gloss;
                        break;
                }
            }
            else if (target is ListBox listBox && listBox == SelectedItemVerseRenderings)
            {
                copyText = listBox.SelectedItem.ToString();
            }
            else if (target is ListView listView && listView == SelectedItemVerses && listView.SelectedItem is VerseViewModel verse)
            {
                copyText = verse.VerseText;
            }
                
            Clipboard.SetText(copyText);
        }
    }
}
