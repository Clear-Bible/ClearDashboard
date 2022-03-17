using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class Adornment
    {
        public long Id { get; set; }
        public long? TokenId { get; set; }
        public string Lemma { get; set; }
        public string Pos { get; set; }
        public string Strong { get; set; }

        public virtual Token Token { get; set; }
    }
}
