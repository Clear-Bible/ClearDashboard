
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
	public class Lexicon_WordAnalysis : SynchronizableTimestampedEntity
	{
		public Lexicon_WordAnalysis()
		{
			// ReSharper disable VirtualMemberCallInConstructor
			Lexemes = new HashSet<Lexicon_Lexeme>();
			// ReSharper restore VirtualMemberCallInConstructor
		}

		public string Language { get; set; } = string.Empty;
		public string Word { get; set; } = string.Empty;
		public ICollection<Lexicon_Lexeme> Lexemes { get; set; }
	}
}
