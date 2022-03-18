using System;
using System.Collections.Generic;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class QuestionGroup
    {
        public string Note { get; set; }
        public string Title { get; set; }
        public string English { get; set; }
        public string AltText { get; set; }
        public double LastChanged { get; set; }
    }
}
