using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Models
{
    [XmlRoot(ElementName = "Status")]
    public class Status
    {

        [XmlAttribute(AttributeName = "Word")]
        public string? Word { get; set; }

        [XmlAttribute(AttributeName = "State")]
        public string State { get; set; }

        [XmlElement(ElementName = "SpecificCase")]
        public string SpecificCase { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Correction")]
        public string Correction { get; set; }
    }

    [XmlRoot(ElementName = "SpellingStatus")]
    public class SpellingStatus
    {

        [XmlElement(ElementName = "Status")]
        public List<Status> Status { get; set; }
    }
}
