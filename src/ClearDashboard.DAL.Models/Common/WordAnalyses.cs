using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Models
{
	[XmlRoot(ElementName = "WordAnalyses")]
	public class WordAnalyses
	{
		[XmlElement(ElementName = "Entry")]
		public List<WordAnalysesEntry> Entries { get; set; } = new();
	}

	[XmlRoot(ElementName = "Entry")]
	public class WordAnalysesEntry
	{
		public Guid Id = Guid.NewGuid();

		[XmlElement(ElementName = "Analysis")]
		public List<WordAnalysis> Analyses { get; set; } = new();

		[XmlAttribute(AttributeName = "Word")]
		public string Word { get; set; } = string.Empty;
	}

	[XmlRoot(ElementName = "Analysis")]
	public class WordAnalysis
	{

		[XmlElement(ElementName = "Lexeme")]
		public List<string> Lexemes { get; set; } = new();
	}
}
