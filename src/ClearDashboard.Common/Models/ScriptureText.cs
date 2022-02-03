using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ClearDashboard.Common.Models
{
    [XmlRoot(ElementName = "ScriptureText")]
    public class ScriptureText
    {

        [XmlElement(ElementName = "FullName")]
        public string FullName { get; set; }

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
