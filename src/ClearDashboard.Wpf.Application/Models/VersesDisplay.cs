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
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Models
{
    public class VersesDisplay : DashboardApplicationScreen
    {
        #region Row0

        private Guid _row0CorpusId;
        public Guid Row0CorpusId
        {
            get => _row0CorpusId;
            set
            {
                _row0CorpusId = value;
                NotifyOfPropertyChange(() => Row0CorpusId);
            }
        }

        private Visibility _row0Visiblity = Visibility.Collapsed;
        public Visibility Row0Visibility
        {
            get => _row0Visiblity;
            set
            {
                _row0Visiblity = value;
                NotifyOfPropertyChange(() => Row0Visibility);
            }
        }

        private Brush _row0BorderColor = Brushes.Blue;
        public Brush Row0BorderColor
        {
            get => _row0BorderColor;
            set
            {
                _row0BorderColor = value;
                NotifyOfPropertyChange(() => Row0BorderColor);
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
        
        private ObservableCollection<List<TokenDisplayViewModel>> _row0Verses = new();
        public ObservableCollection<List<TokenDisplayViewModel>> Row0Verses
        {
            get => _row0Verses;
            set => Set(ref _row0Verses, value);
        }

        private bool _row0ShowTranslation;

        public bool Row0ShowTranslation
        {
            get => _row0ShowTranslation;
            set => Set(ref _row0ShowTranslation, value);
        }


        #endregion Row0



        #region Row1

        private Guid _row1CorpusId;
        public Guid Row1CorpusId
        {
            get => _row1CorpusId;
            set
            {
                _row1CorpusId = value;
                NotifyOfPropertyChange(() => Row1CorpusId);
            }
        }

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

        private Brush _row1BorderColor = Brushes.Blue;
        public Brush Row1BorderColor
        {
            get => _row1BorderColor;
            set
            {
                _row1BorderColor = value;
                NotifyOfPropertyChange(() => Row1BorderColor);
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

        private bool _row1ShowTranslation;

        public bool Row1ShowTranslation
        {
            get => _row1ShowTranslation;
            set => Set(ref _row1ShowTranslation, value);
        }

        #endregion Row1



        #region Row2

        private Guid _row2CorpusId;
        public Guid Row2CorpusId
        {
            get => _row2CorpusId;
            set
            {
                _row2CorpusId = value;
                NotifyOfPropertyChange(() => Row2CorpusId);
            }
        }

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

        private Brush _row2BorderColor = Brushes.Blue;
        public Brush Row2BorderColor
        {
            get => _row2BorderColor;
            set
            {
                _row2BorderColor = value;
                NotifyOfPropertyChange(() => Row2BorderColor);
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

        private bool _row2ShowTranslation;

        public bool Row2ShowTranslation
        {
            get => _row2ShowTranslation;
            set => Set(ref _row2ShowTranslation, value);
        }

        #endregion Row2



        #region Row3

        private Guid _row3CorpusId;
        public Guid Row3CorpusId
        {
            get => _row3CorpusId;
            set
            {
                _row3CorpusId = value;
                NotifyOfPropertyChange(() => Row3CorpusId);
            }
        }

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

        private Brush _row3BorderColor = Brushes.Blue;
        public Brush Row3BorderColor
        {
            get => _row3BorderColor;
            set
            {
                _row3BorderColor = value;
                NotifyOfPropertyChange(() => Row3BorderColor);
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

        private ObservableCollection<List<TokenDisplayViewModel>>? _row3Verses = new();
        public ObservableCollection<List<TokenDisplayViewModel>>? Row3Verses
        {
            get => _row3Verses;
            set => Set(ref _row3Verses, value);
        }

        private bool _row3ShowTranslation;

        public bool Row3ShowTranslation
        {
            get => _row3ShowTranslation;
            set => Set(ref _row3ShowTranslation, value);
        }
        
        #endregion Row3
    }
}
