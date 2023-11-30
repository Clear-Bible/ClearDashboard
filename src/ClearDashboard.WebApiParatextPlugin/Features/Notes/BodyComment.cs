using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    [DataContract]
    internal class BodyComment
    {
        [DataMember]
        public List<string> Paragraphs { get; set; }
        [DataMember]
        public string Created { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string AssignedUserName { get; set; }
        [DataMember]
        public string Author { get; set; }
    }
}
