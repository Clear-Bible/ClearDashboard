

namespace ClearDashboard.DataAccessLayer.Models
{
    public class VerseRefStart 
    {
     
        public string? BookCode { get; set; }

       
        public int BookNum { get; set; }

        
        public int ChapterNum { get; set; }

        
        public int VerseNum { get; set; }

        
        public int Bbbcccvvv { get; set; }

        
        public Versification? Versification { get; set; }

      
        public bool RepresentsMultipleVerses { get; set; }


        public List<object> AllVerses { get; set; } = new List<object>();
    }
}