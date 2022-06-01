using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    [XmlRoot(ElementName = "References")]
    public class References
    {

        [XmlElement(ElementName = "Verse")]
        public List<string> Verse { get; set; }
    }

    [XmlRoot(ElementName = "Term")]
    public class Term
    {
        [XmlElement(ElementName = "Strong")]
        public string Strong { get; set; }

        [XmlElement(ElementName = "Transliteration")]
        public string Transliteration { get; set; }

        [XmlElement(ElementName = "Category")]
        public string Category { get; set; }

        [XmlElement(ElementName = "Domain")]
        public string Domain { get; set; }

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "Definition")]
        public string Definition { get; set; }

        [XmlElement(ElementName = "Gloss")]
        public string Gloss { get; set; }

        [XmlElement(ElementName = "References")]
        public References References { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Link")]
        public string Link { get; set; }
    }

    [XmlRoot(ElementName = "BiblicalTermsList")]
    public class BiblicalTermsList
    {

        [XmlElement(ElementName = "Term")]
        public List<Term> Term { get; set; }

        [XmlAttribute(AttributeName = "xsi")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd")]
        public string Xsd { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
