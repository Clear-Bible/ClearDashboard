using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
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
