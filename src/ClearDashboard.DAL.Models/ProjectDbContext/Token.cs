using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class Token : TokenComponent
    {
        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }
        public int VerseNumber { get; set; }
        public int WordNumber { get; set; }
        public int SubwordNumber { get; set; }

        public string? SurfaceText { get; set; }
        public string? PropertiesJson { get; set; }

        public virtual Adornment? Adornment { get; set; }

        public Guid? TokenCompositeId { get; set; }
        public virtual TokenComposite? TokenComposite { get; set; }
    }
}
