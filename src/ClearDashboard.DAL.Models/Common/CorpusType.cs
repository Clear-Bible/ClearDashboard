namespace ClearDashboard.DataAccessLayer.Models;

public enum CorpusType
{
    Unknown,
    Standard,
    Resource,
    BackTranslation,
    Daughter,
    MarbleResource,

    TransliterationManual,
    TransliterationWithEncoder,
    StudyBible,
    ConsultantNotes,
    StudyBibleAdditions,
    Auxiliary,
    Xml,
    SourceLanguage,
    Dictionary,
    EnhancedResource,
}

public enum CorpusSourceType
{
    Manuscript,
    // ReSharper disable once InconsistentNaming
    USFM,
    Paratext
}