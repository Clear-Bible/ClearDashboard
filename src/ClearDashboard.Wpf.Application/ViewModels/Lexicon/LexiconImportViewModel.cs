
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconImportViewModel : PropertyChangedBase
    {
        private bool _isSelected;
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
