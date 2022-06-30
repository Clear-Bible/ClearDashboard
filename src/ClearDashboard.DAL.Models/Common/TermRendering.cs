using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Models
{

    [XmlRoot(ElementName = "TermRendering")]
    public class TermRendering
    {

        [XmlElement(ElementName = "Changes")]
        public Changes? Changes { get; set; }

        [XmlElement(ElementName = "Notes")]
        public object Notes { get; set; }

        [XmlElement(ElementName = "Denials")]
        public Denials Denials { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "Guess")]
        public bool Guess { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Renderings")]
        public string Renderings { get; set; }

        [XmlElement(ElementName = "Glossary")]
        public object Glossary { get; set; }
    }

    [XmlRoot(ElementName = "Change")]
    public class Change
    {

        [XmlElement(ElementName = "Before")]
        public object Before { get; set; }

        [XmlElement(ElementName = "After")]
        public string After { get; set; }

        [XmlAttribute(AttributeName = "UserName")]
        public string UserName { get; set; }

        [XmlAttribute(AttributeName = "TermId")]
        public string TermId { get; set; }

        [XmlAttribute(AttributeName = "Date")]
        public DateTime Date { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Changes")]
    public class Changes
    {

        [XmlElement(ElementName = "Change")]
        public List<Change> Change { get; set; }
    }

    [XmlRoot(ElementName = "Denials")]
    public class Denials
    {

        [XmlElement(ElementName = "Denial")]
        public List<int> Denial { get; set; }
    }

    [XmlRoot(ElementName = "TermRenderingsList")]
    public class TermRenderingsList
    {

        [XmlElement(ElementName = "TermRendering")]
        public List<TermRendering> TermRendering { get; set; }
    }


}
