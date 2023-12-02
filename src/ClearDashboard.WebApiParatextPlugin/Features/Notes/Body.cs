using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    [DataContract]
    public class Body
    {
        [DataMember]
        public string AssignedUserName { get; set; }
        [DataMember]
        public string ReplyToUserName { get; set; }
        [DataMember]
        public bool IsRead { get; set; }
        [DataMember]
        public bool IsResolved { get; set; }
        [DataMember]
        public List<BodyComment> Comments { get; set; }
        [DataMember]
        public string VerseUsfmBeforeSelectedText { get; set; }
        [DataMember]
        public string VerseUsfmAfterSelectedText { get; set; }
        [DataMember]
        public string VerseUsfmText { get; set; }

        internal List<ExternalNoteMessage> GetExternalNoteMessages()
        {
            return Comments
                .Select(c => new ExternalNoteMessage()
                {
                    Created = DateTime.Parse(c.Created),
                    Language = c.Language,
                    ExternalUserNameAuthoredBy = c.Author,
                    Text = c.Paragraphs.Aggregate("", (innerstring, next) => $"{innerstring}{next}{Environment.NewLine}")
                })
                .ToList();
        }

        internal string SerializeNoteBodyXml()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Body>));
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, new List<Body> { this });
                return textWriter.ToString();
            }
        }
        internal string SerializeNoteBodyJson()
        {
            //from https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN for
            //.net 4.x
            // Create a stream to serialize the object to.
            var ms = new MemoryStream();

            // Serializer the User object to the stream.
            var ser = new DataContractJsonSerializer(typeof(Body));
            ser.WriteObject(ms, this);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
    }
}
