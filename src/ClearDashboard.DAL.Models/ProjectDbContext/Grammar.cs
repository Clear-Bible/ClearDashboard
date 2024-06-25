using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;


    public class Grammar : IdentifiableEntity
    {
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
