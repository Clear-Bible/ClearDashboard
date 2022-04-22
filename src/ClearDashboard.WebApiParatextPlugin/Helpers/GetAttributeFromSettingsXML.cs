using System.Xml;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class GetAttributeFromSettingsXML
    {
        public static string GetValue(string xmlPath, string Attribute)
        {
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            XmlElement root = doc.DocumentElement;
            var node = root.SelectSingleNode($"//{Attribute}");

            if (node != null)
            {
                return node.InnerText;
            }

            return "";
        }
    }
}
