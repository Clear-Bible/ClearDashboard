﻿using System.Xml.Serialization;

namespace ClearDashboard.Common.Models
{
    [XmlRoot(ElementName = "ScriptureText")]
    public class ScriptureText
    {

        [XmlElement(ElementName = "FullName")]
        public string FullName { get; set; }

        [XmlElement(ElementName = "Guid")]
        public string Guid { get; set; }

        [XmlElement(ElementName = "NormalizationForm")]
        public string NormalizationForm { get; set; }

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "DefaultFont")]
        public string DefaultFont { get; set; }

        [XmlElement(ElementName = "Encoding")]
        public string Encoding { get; set; }

        [XmlElement(ElementName = "LanguageIsoCode")]
        public string LanguageIsoCode { get; set; }

        [XmlElement(ElementName = "BaseTranslation")]
        public string BaseTranslation { get; set; }

        [XmlElement(ElementName = "TranslationInfo")]
        public string TranslationInfo { get; set; }

        [XmlElement(ElementName = "Copyright")]
        public string Copyright { get; set; }

        [XmlElement(ElementName = "FileNameBookNameForm")]
        public string FileNameBookNameForm { get; set; }

        [XmlElement(ElementName = "FileNamePrePart")]
        public object FileNamePrePart { get; set; }

        [XmlElement(ElementName = "FileNamePostPart")]
        public string FileNamePostPart { get; set; }

        [XmlElement(ElementName = "BooksPresent")]
        public string BooksPresent { get; set; }

        [XmlElement(ElementName = "Versification")]
        public int Versification { get; set; }

        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "LeftToRight")]
        public string LeftToRight { get; set; }

    }
}
