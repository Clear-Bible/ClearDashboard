using Newtonsoft.Json;
using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.NamedPipes.Models
{
    public class Content : ObservableObject
    {
        private List<Span> _Spans;
        [JsonProperty]
        public List<Span> Spans
        {
            get => _Spans;
            set { SetProperty(ref _Spans, value); }
        }

    }
}
