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
        private Guid _corpusId;
        public Guid CorpusId
        {
            get => _corpusId;
            set
            {
                _corpusId = value;
                NotifyOfPropertyChange(() => CorpusId);
            }
        }


        private Guid _alignmentSetId;
        public Guid AlignmentSetId
        {
            get => _alignmentSetId;
            set
            {
                _alignmentSetId = value;
                NotifyOfPropertyChange(() => AlignmentSetId);
            }
        }


        private Guid _parallelCorpusId;
        public Guid ParallelCorpusId
        {
            get => _parallelCorpusId;
            set
            {
                _parallelCorpusId = value;
                NotifyOfPropertyChange(() => ParallelCorpusId);
            }
        }

        private Guid _translationSetId;
        public Guid TranslationSetId
        {
            get => _translationSetId;
            set
            {
                _translationSetId = value;
                NotifyOfPropertyChange(() => TranslationSetId);
            }
        }



        private Visibility _visiblity = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => _visiblity;
            set
            {
                _visiblity = value;
                NotifyOfPropertyChange(() => Visibility);
            }
        }

        
        private Brush _borderColor = Brushes.Blue;
        public Brush BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                NotifyOfPropertyChange(() => BorderColor);
            }
        }

        
        private string _rowTitle = string.Empty;
        public string RowTitle
        {
            get => _rowTitle;
            set
            {
                _rowTitle = value;
                NotifyOfPropertyChange(() => RowTitle);
            }
        }
        
        
        private ObservableCollection<VerseDisplayViewModel> _verses = new();
        public ObservableCollection<VerseDisplayViewModel> Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

        
        private FontFamily _fontFamily;
        public FontFamily FontFamily
        {
            get => _fontFamily;
            set => Set(ref _fontFamily, value);
        }

        private FontFamily _translationFontFamily;
        public FontFamily TranslationFontFamily
        {
            get => _translationFontFamily;
            set => Set(ref _translationFontFamily, value);
        }


        private bool _showTranslation;
        public bool ShowTranslation
        {
            get => _showTranslation;
            set => Set(ref _showTranslation, value);
        }

        private bool _isRtl;

        public bool IsRtl
        {
            get => _isRtl;
            set => Set(ref _isRtl, value);
        }


    }
}
