namespace ClearDashboard.DataAccessLayer.Models;

public enum CorpusType
{
    Unknown,
    Standard,
    Resource,
    BackTranslation,
    Auxiliary,
    Daughter,
    MarbleResource,
}

public enum CorpusSourceType
{
    Manuscript,
    // ReSharper disable once InconsistentNaming
    USFM,
    Paratext
}