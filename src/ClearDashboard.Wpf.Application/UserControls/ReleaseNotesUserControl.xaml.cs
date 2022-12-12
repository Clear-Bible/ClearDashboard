using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
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
using ClearDashboard.Wpf.Application.Helpers;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for ReleaseNotesUserControl.xaml
    /// </summary>
    public partial class ReleaseNotesUserControl : UserControl
    {

        private UpdateFormat? _updateData;

        private string? _version;
        public string? Version
        {
            get => _version;
            set { _version = value; }
        }

        private Visibility _showUpdateLink = Visibility.Collapsed;
        public Visibility ShowUpdateLink
        {
            get => _showUpdateLink;
            set { _showUpdateLink = value; }
        }


        private Uri _updateUrl = new Uri("https://www.clear.bible");
        public Uri UpdateUrl
        {
            get => _updateUrl;
            set { _updateUrl = value; }
        }

        private int myVar;

        public List<ReleaseNote> UpdateNotes { get; set; }

        public List<UpdateFormat> Updates { get; set; }

        public ReleaseNotesUserControl()
        {
            InitializeComponent();
            CheckForProgramUpdates();
        }

        private async void CheckForProgramUpdates()
        {

            var fullUpdateDataList = await ReleaseNotesManager.GetUpdateDataFromFile();

            if (fullUpdateDataList != null)
            {
                var isNewer = ReleaseNotesManager.CheckWebVersion(fullUpdateDataList.FirstOrDefault().Version);

                if (isNewer)
                {
                    ShowUpdateLink = Visibility.Visible;
                    UpdateUrl = new Uri(fullUpdateDataList.FirstOrDefault().DownloadLink);

                    Updates = await ReleaseNotesManager.GetRelevantUpdates(fullUpdateDataList);
                    //UpdateNotes = await ReleaseNotesManager.GetUpdateNotes(fullUpdateDataList);//could replace with ReleaseManager.UpdateNotes to speed up
                }
            }
        }

        private void ClickUpdateLink_OnClick(object sender, RoutedEventArgs e)
        {
            RunClickUpdateLink();
        }

        public void RunClickUpdateLink()
        {
            if (UpdateUrl.AbsoluteUri == "")
            {
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = UpdateUrl.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private void ShowNotes_OnClick(object sender, RoutedEventArgs e)
        {
            RunShowNotes();
        }

        public void RunShowNotes()
        {
            //var localizedString = LocalizationStrings.Get("ShellView_ShowNotes", Logger); TODO
            var titleString = "Release Notes";

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.CanResize;
            settings.MinWidth = 800;
            settings.MinHeight = 600;
            settings.Title = $"{titleString} - {_updateData?.Version}";

            var viewModel = IoC.Get<ShowUpdateNotesViewModel>();
            viewModel.Updates = new ObservableCollection<UpdateFormat>(Updates);

            IWindowManager manager = new WindowManager();
            manager.ShowWindowAsync(viewModel, null, settings);
        }
    }
}
