namespace ParaTextPlugin.Data.Models
{
    public interface IEnumType
    {
    }

    public class ProjectType : IEnumType
    {
        public static readonly Enum<ProjectType> Standard = new Enum<ProjectType>(nameof(Standard));
        public static readonly Enum<ProjectType> Resource = new Enum<ProjectType>(nameof(Resource));
        public static readonly Enum<ProjectType> BackTranslation = new Enum<ProjectType>(nameof(BackTranslation));
        public static readonly Enum<ProjectType> Daughter = new Enum<ProjectType>(nameof(Daughter));
        public static readonly Enum<ProjectType> TransliterationManual = new Enum<ProjectType>("Transliteration");
        public static readonly Enum<ProjectType> TransliterationWithEncoder = new Enum<ProjectType>(nameof(TransliterationWithEncoder));
        public static readonly Enum<ProjectType> StudyBible = new Enum<ProjectType>(nameof(StudyBible));
        public static readonly Enum<ProjectType> ConsultantNotes = new Enum<ProjectType>(nameof(ConsultantNotes));
        public static readonly Enum<ProjectType> GlobalConsultantNotes = new Enum<ProjectType>(nameof(GlobalConsultantNotes));
        public static readonly Enum<ProjectType> GlobalAnthropologyNotes = new Enum<ProjectType>(nameof(GlobalAnthropologyNotes));
        public static readonly Enum<ProjectType> StudyBibleAdditions = new Enum<ProjectType>(nameof(StudyBibleAdditions));
        public static readonly Enum<ProjectType> Auxiliary = new Enum<ProjectType>(nameof(Auxiliary));
        public static readonly Enum<ProjectType> AuxiliaryResource = new Enum<ProjectType>(nameof(AuxiliaryResource));
        public static readonly Enum<ProjectType> MarbleResource = new Enum<ProjectType>(nameof(MarbleResource));
        public static readonly Enum<ProjectType> XmlResource = new Enum<ProjectType>(nameof(XmlResource));
        public static readonly Enum<ProjectType> XmlDictionary = new Enum<ProjectType>(nameof(XmlDictionary));
        public static readonly Enum<ProjectType> NotSelected = new Enum<ProjectType>(nameof(NotSelected));

        private ProjectType()
        {
        }
    }
}
