using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Adornment
    {
        public int Id { get; set; }
        public int? TokenId { get; set; }
        public string Lemma { get; set; }
        public string PartsOfSpeech { get; set; }
        public string Strong { get; set; }

        public virtual Token Token { get; set; }
    }
}
