using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Views.Project
{
    /// <summary>
    /// Interaction logic for EnhancedCorpusView.xaml
    /// </summary>
    public partial class EnhancedView : UserControl
    {
        public EnhancedView()
        {
            InitializeComponent();
        }



        private void InnerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var verseDisplayViewModel = e.AddedItems[0] as VerseDisplayViewModel;
                Debug.WriteLine(verseDisplayViewModel.Id);

                var vm = DataContext as EnhancedViewModel;
                vm.SelectedVerseDisplayViewModel = verseDisplayViewModel;
            }

        }
    }
}
