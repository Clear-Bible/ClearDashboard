
using System;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class LexiconImportViewModel : PropertyChangedBase
    {
        private Guid _lexemeId;
        private Guid _meaningId;
        private Guid _translationId;
        private bool _isSelected;
        private bool _hasConflictingMatch;
        private bool _showAddAsFormButton;
        private bool _showAddTargetAsTranslationButton;

        private string? _sourceWord;
        private string? _sourceLanguage;
        private string? _sourceType;

        private string? _targetWord;
        private string? _targetLanguage;


        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public bool HasConflictingMatch
        {
            get => _hasConflictingMatch;
            set => Set(ref _hasConflictingMatch, value);
        }

        public Guid LexemeId
        {
            get => _lexemeId;
            set => Set(ref _lexemeId, value);
        }

        public Guid MeaningId
        {
            get => _meaningId;
            set => Set(ref _meaningId, value);
        }

        public Guid TranslationId
        {
            get => _translationId;
            set => Set(ref _translationId, value);
        }

        public bool ShowAddAsFormButton
        {
            get => _showAddAsFormButton;
            set => Set(ref _showAddAsFormButton, value);
        }

        public bool ShowAddTargetAsTranslationButton
        {
            get => _showAddTargetAsTranslationButton;
            set => Set(ref _showAddTargetAsTranslationButton, value);
        }

        public string? SourceWord
        {
            get => _sourceWord;
            set => Set(ref _sourceWord, value);
        }

        public string? SourceType
        {
            get => _sourceType;
            set => Set(ref _sourceType, value);
        }

        public string? SourceLanguage
        {
            get => _sourceLanguage;
            set => Set(ref _sourceLanguage, value);
        }

        public string? TargetWord
        {
            get => _targetWord;
            set => Set(ref _targetWord, value);
        }

        public string? TargetLanguage
        {
            get => _targetLanguage;
            set => Set(ref _targetLanguage, value);
        }
    }
}
