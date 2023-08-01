using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Models;

[XmlRoot(ElementName = "RegistrationData")]
public class RegistrationData
{

    [XmlElement(ElementName = "Name")]
    public string Name { get; set; }

    [XmlElement(ElementName = "Code")]
    public string Code { get; set; }

    [XmlElement(ElementName = "Email")]
    public string Email { get; set; }

    [XmlElement(ElementName = "SupporterName")]
    public string SupporterName { get; set; }
}