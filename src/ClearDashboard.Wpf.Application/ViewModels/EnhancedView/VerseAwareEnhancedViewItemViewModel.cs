using Caliburn.Micro;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class TestEnhancedViewItemViewModel : EnhancedViewItemViewModel
    {
        public string? Message { get; set; }

        public TestEnhancedViewItemViewModel()
        {
            Title = "TestEnhancedViewItemViewModel";
            Message = "This message was created in the TestEnhancedViewItemViewModel constructor.";
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }
    }


    public class VerseAwareEnhancedViewItemViewModel : EnhancedViewItemViewModel
    {
        private Guid _corpusId;
        public Guid CorpusId
        {
            get => _corpusId;
            set => Set(ref _corpusId, value);
        }


        private Guid _alignmentSetId;
        public Guid AlignmentSetId
        {
            get => _alignmentSetId;
            set => Set(ref _alignmentSetId, value);
        }


        private Guid _parallelCorpusId;
        public Guid ParallelCorpusId
        {
            get => _parallelCorpusId;
            set => Set(ref _parallelCorpusId, value);
        }

        private Guid _translationSetId;
        public Guid TranslationSetId
        {
            get => _translationSetId;
            set => Set(ref _translationSetId, value);
        }

        private BindableCollection<VerseDisplayViewModel> _verses = new();
        public BindableCollection<VerseDisplayViewModel> Verses
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
