using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SIL.IO.FileLock;

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
            var menuItem = (MenuItem)sender;
            var parent = menuItem.Parent;
            var contextMenu = (ContextMenu)parent;
            var target = contextMenu.PlacementTarget;
            var item = (DataGrid)target;

            var columnIndex = item.CurrentColumn.DisplayIndex;

            var cells = item.SelectedCells;
            var selectedItem = cells[0].Item;
            var BiblicalTermsItem = (BiblicalTermsData)selectedItem;
            string copyText;
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
            Clipboard.SetText(copyText);
        }
    }
}
