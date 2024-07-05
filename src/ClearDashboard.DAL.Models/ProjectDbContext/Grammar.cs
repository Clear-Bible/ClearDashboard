using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;


    public class Grammar : IdentifiableEntity
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public Guid Id { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }

        public int Precedence { get; set; }
    }
