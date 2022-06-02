using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    [XmlRoot(ElementName = "Lexeme")]
    public class Lexeme
    {

        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "Form")]
        public string Form { get; set; }

        [XmlAttribute(AttributeName = "Homograph")]
        public int Homograph { get; set; }
    }

    [XmlRoot(ElementName = "Gloss")]
    public class Gloss
    {

        [XmlAttribute(AttributeName = "Language")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Sense")]
    public class Sense
    {

        [XmlElement(ElementName = "Gloss")]
        public Gloss Gloss { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Entry")]
    public class Entry
    {

        [XmlElement(ElementName = "Sense")]
        public List<Sense> Sense { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {

        [XmlElement(ElementName = "Lexeme")]
        public Lexeme Lexeme { get; set; }

        [XmlElement(ElementName = "Entry")]
        public Entry Entry { get; set; }
    }

    [XmlRoot(ElementName = "Entries")]
    public class Entries
    {

        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "Lexicon")]
    public class Lexicon
    {

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "FontName")]
        public string FontName { get; set; }

        [XmlElement(ElementName = "FontSize")]
        public int FontSize { get; set; }

        [XmlElement(ElementName = "Analyses")]
        public object Analyses { get; set; }

        [XmlElement(ElementName = "Entries")]
        public Entries Entries { get; set; }
    }
}
