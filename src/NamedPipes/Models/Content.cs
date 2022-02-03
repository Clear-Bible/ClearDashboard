using Newtonsoft.Json;
using System.Collections.Generic;
using ClearDashboard.Common.Models;

namespace ClearDashboard.NamedPipes.Models
{
    public class Content : BindableBase
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
