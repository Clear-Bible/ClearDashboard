using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

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



        private Visibility _visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
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


        #region FontFamily

        private FontFamily? _targetFontFamily;
        public FontFamily? TargetFontFamily
        {
            get => _targetFontFamily;
            set => Set(ref _targetFontFamily, value);
        }

        private FontFamily? _sourceFontFamily;
        public FontFamily? SourceFontFamily
        {
            get => _sourceFontFamily;
            set => Set(ref _sourceFontFamily, value);
        }

        private FontFamily? _translationFontFamily;
        public FontFamily? TranslationFontFamily
        {
            get => _translationFontFamily;
            set => Set(ref _translationFontFamily, value);
        }

        #endregion


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

        private bool _isTargetRtl;

        public bool IsTargetRtl
        {
            get => _isTargetRtl;
            set => Set(ref _isTargetRtl, value);
        }
    }
}
