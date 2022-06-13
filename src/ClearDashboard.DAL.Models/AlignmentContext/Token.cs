namespace ClearDashboard.DataAccessLayer.Models
{
    public  class Token
    {
        public Token()
        {
            }

        public int Id { get; set; }

        // add unique constraint for WordNumber and SubwordNumber
        public int WordNumber { get; set; }
        public int SubwordNumber { get; set; }

        public int VerseId { get; set; }
        public string? Text { get; set; }
        public string? FirstLetter { get; set; }

       // public Alignment Alignment { get; set; }
        //public virtual Alignment TokenNavigation { get; set; }
        public Adornment? Adornment { get; set; }
        public Verse? Verse { get; set; }
        //public virtual ICollection<InterlinearNote> InterlinearNotes { get; set; }
    }
}
