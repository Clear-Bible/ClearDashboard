using System.ComponentModel;
using System.Reflection;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class PinsDataTable
    {
        // 0. Source, 1. Lform, 2. Gloss, 3. Lang, 4. Refs, 5. Code, 6. Match, 7. Notes, 8. SimpRefs, 
        // 9. Phrase, 10. Word, 11. Prefix, 12. Stem, 13. Suffix
        public Guid Id { get; set; }
        public string OriginID { get; set; } = string.Empty;
        public XmlSource XmlSource { get; set; }
        public string XmlSourceAbbreviation { get; set; } = string.Empty;
        public string XmlSourceDisplayName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Lform { get; set; } = string.Empty;
        public string Gloss { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;
        public string Refs { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Match { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string SimpRefs { get; set; } = string.Empty;
        public string LexemeType { get; set; } = string.Empty;
        public string Phrase { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string Stem { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public List<string> VerseList { get; set; } = new ();

    }

    public enum XmlSource
    {
        [Description("All")]
        All,
        [Description("KT")]
        BiblicalTerms,
        [Description("ABT")]
        AllBiblicalTerms,
        [Description("TR")]
        TermsRenderings,
        [Description("LX")]
        Lexicon,
    }

    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum GenericEnum)
        {
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }

    }
}
