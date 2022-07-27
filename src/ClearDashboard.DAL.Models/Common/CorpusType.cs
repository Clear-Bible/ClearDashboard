namespace ClearDashboard.DataAccessLayer.Models;

public enum CorpusType
{
    /// <summary>A standard translation with no base project</summary>
    Standard,
    /// <summary>A back translation of another translation</summary>
    BackTranslation,
    /// <summary>A translation based on a front translation</summary>
    Daughter,
    /// <summary>A manual transliteration of a text</summary>
    TransliterationManual,
    /// <summary>An automated transliteration of a text</summary>
    TransliterationWithEncoder,
    /// <summary>A study Bible based on an original text</summary>
    StudyBible,
    /// <summary>Collection of consultant notes</summary>
    ConsultantNotes,
    /// <summary>A study Bible additions project</summary>
    StudyBibleAdditions,
    /// <summary>A standard project that goes with a base project used for anything related to the project that
    /// doesn't fit into another derived type.</summary>
    Auxiliary,
    /// <summary>A project stored in xml format</summary>
    Xml,
    /// <summary>A source language project</summary>
    SourceLanguage,
    /// <summary>A dictionary project</summary>
    Dictionary,
    /// <summary>NB:  this is an Enhanced Resource in Paratext - a published resource that has text mapped to Hebrew/Greek source text.</summary>
    MarbleResource,


    // NB:  CODE REVIEW:  Do we still need this WRT to ParatextProxy lines 264 & 439, CreateNewProjectWorkflowStepViewModel line 315?
    Resource,
    Unknown
}

public enum CorpusSourceType
{
    Manuscript,
    // ReSharper disable once InconsistentNaming
    USFM,
    Paratext
}