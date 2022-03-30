using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Content : ObservableObject
    {
        private List<Span> _Spans;
        public List<Span> Spans
        {
            get => _Spans;
            set { SetProperty(ref _Spans, value); }
        }

    }
}
