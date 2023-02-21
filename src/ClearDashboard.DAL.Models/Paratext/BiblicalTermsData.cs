namespace ClearDashboard.DataAccessLayer.Models
{
    public class BiblicalTermsData 
    {
        public string? Id { get; set; }
        
        public string? Lemma { get; set; }
        
        public string? Transliteration { get; set; }
        
        public string? SemanticDomain { get; set; }
       
        public string? LocalGloss { get; set; }
        
        public string? Gloss { get; set; }
        
        public string? LinkString { get; set; }

        public List<string> References { get; set; } = new List<string>();
        
        public List<string> ReferencesLong { get; set; } = new List<string>();
        
        public List<string> ReferencesListText { get; set; } = new List<string>();
        
        public List<string> Renderings { get; set; } = new List<string>();
        
        public string? RenderingString { get; set; }
        
        public int RenderingCount { get; set; }

        public string Counts
        {
            get => $"{RenderingCount}/{ReferencesLong.Count}";
        }

        public bool Found { get; set; }

    }

}
