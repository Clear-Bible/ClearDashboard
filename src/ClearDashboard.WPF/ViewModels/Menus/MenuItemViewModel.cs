using Caliburn.Micro;
using ClearDashboard.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Models.Menus
{
    public class MenuItemViewModel : PropertyChangedBase
    {
        private readonly ICommand _command;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }

        private WorkSpaceViewModel _workSpaceViewModel;

        public WorkSpaceViewModel ViewModel
        {
            get { return _workSpaceViewModel; }
            set { _workSpaceViewModel = value; }
        }



        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }



        private string _Id;
        public string Id
        {
            get => _Id;
            set
            {
                _Id = value;
                NotifyOfPropertyChange(() => Id);
            }
        }


        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                NotifyOfPropertyChange(() => Header);
            }
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        private void Execute()
        {
            if (ViewModel != null)
            {

                Debug.WriteLine(Id);
                switch (Id)
                {
                    case "LayoutID":
                        break;
                    case "AlignmentToolID":
                        ViewModel.WindowIDToLoad = "ALIGNMENTTOOL";
                        break;
                    case "BiblicalTermsID":
                        ViewModel.WindowIDToLoad = "BIBLICALTERMS";
                        break;
                    case "ConcordanceToolID":
                        ViewModel.WindowIDToLoad = "CONCORDANCETOOL";
                        break;
                    case "DashboardID":
                        ViewModel.WindowIDToLoad = "DASHBOARD";
                        break;
                    case "NotesID":
                        ViewModel.WindowIDToLoad = "NOTES";
                        break;
                    case "PINSID":
                        ViewModel.WindowIDToLoad = "PINS";
                        break;
                    case "WordMeaningsID":
                        ViewModel.WindowIDToLoad = "WORDMEANINGS";
                        break;
                    case "SourceContextID":
                        ViewModel.WindowIDToLoad = "SOURCECONTEXT";
                        break;
                    case "StartPageID":
                        ViewModel.WindowIDToLoad = "STARTPAGE";
                        break;
                    case "TargetContextID":
                        ViewModel.WindowIDToLoad = "TARGETCONTEXT";
                        break;
                    case "TextCollectionID":
                        ViewModel.WindowIDToLoad = "TEXTCOLLECTION";
                        break;
                }
            }
        }
    }
}
