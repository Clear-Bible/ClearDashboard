using ClearBible.Engine.Corpora;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Models
{
    public class VersesDisplay : DashboardApplicationScreen
    {
        #region Row0

        private Visibility _Row0Visiblity = Visibility.Collapsed;
        public Visibility Row0Visibility
        {
            get => _Row0Visiblity;
            set
            {
                _Row0Visiblity = value;
                NotifyOfPropertyChange(() => Row0Visibility);
            }
        }

        private string _row0title = string.Empty;
        public string Row0Title
        {
            get => _row0title;
            set
            {
                _row0title = value;
                NotifyOfPropertyChange(() => Row0Title);
            }
        }
        
        private ObservableCollection<List<TokenDisplayViewModel>>? _row0Verses = new();
        public ObservableCollection<List<TokenDisplayViewModel>>? Row0Verses
        {
            get => _row0Verses;
            set => Set(ref _row0Verses, value);
        }

        #endregion Row0



        #region Row1

        private string _row1title = string.Empty;
        public string Row1Title
        {
            get => _row1title;
            set
            {
                _row1title = value;
                NotifyOfPropertyChange(() => Row1Title);
            }
        }

        private Visibility _Row1Visiblity = Visibility.Collapsed;
        public Visibility Row1Visibility
        {
            get => _Row1Visiblity;
            set
            {
                _Row1Visiblity = value;
                NotifyOfPropertyChange(() => Row1Visibility);
            }
        }

        private ObservableCollection<List<TokenDisplayViewModel>>? _row1Verses = new();
        public ObservableCollection<List<TokenDisplayViewModel>>? Row1Verses
        {
            get => _row1Verses;
            set => Set(ref _row1Verses, value);
        }

        #endregion Row1



        #region Row2

        private Visibility _Row2Visiblity = Visibility.Collapsed;
        public Visibility Row2Visibility
        {
            get => _Row2Visiblity;
            set
            {
                _Row2Visiblity = value;
                NotifyOfPropertyChange(() => Row2Visibility);
            }
        }

        private string _row2title = string.Empty;
        public string Row2Title
        {
            get => _row2title;
            set
            {
                _row2title = value;
                NotifyOfPropertyChange(() => Row2Title);
            }
        }

        private ObservableCollection<List<TokenDisplayViewModel>>? _row2Verses = new();
        public ObservableCollection<List<TokenDisplayViewModel>>? Row2Verses
        {
            get => _row2Verses;
            set => Set(ref _row2Verses, value);
        }

        #endregion Row2



        #region Row3

        private Visibility _Row3Visiblity = Visibility.Collapsed;
        public Visibility Row3Visibility
        {
            get => _Row3Visiblity;
            set
            {
                _Row3Visiblity = value;
                NotifyOfPropertyChange(() => Row3Visibility);
            }
        }

        private string _row3title = string.Empty;
        public string Row3Title
        {
            get => _row3title;
            set
            {
                _row3title = value;
                NotifyOfPropertyChange(() => Row3Title);
            }
        }

        private ObservableCollection<List<TokenDisplayViewModel>>? _row3Verses;
        public ObservableCollection<List<TokenDisplayViewModel>>? Row3Verses
        {
            get => _row3Verses;
            set => Set(ref _row3Verses, value);
        }

        #endregion Row3
    }
}
