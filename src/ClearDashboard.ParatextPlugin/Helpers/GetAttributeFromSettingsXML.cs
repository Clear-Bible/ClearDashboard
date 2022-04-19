using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClearDashboard.ParatextPlugin.Helpers
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
