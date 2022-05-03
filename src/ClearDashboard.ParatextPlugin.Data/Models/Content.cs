using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.ParatextPlugin.Data.Models
{
    public class Content : ObservableObject
    {
        private List<Span> _spans;
        public List<Span> Spans
        {
            get => _spans;
            set => SetProperty(ref _spans, value);
        }

    }
}
