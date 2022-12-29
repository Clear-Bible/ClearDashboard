using ClearBible.Engine.Utils;
using System.Text.Json.Serialization;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class CorpusId : EntityId<CorpusId>, IEquatable<CorpusId>
    {
        public CorpusId(Guid id)
        {
            Id = id;
        }

        public CorpusId(
            Guid id, bool isRtl, string? fontFamily, string? name, string? displayName, 
            string? language, string? paratextGuid, string? corpusType, Dictionary<string, object> metadata,
            DateTimeOffset created, UserId userId)
        {
            Id = id;
            IsRtl = isRtl;
            FontFamily = fontFamily;
            Name = name;
            DisplayName = displayName;
            Language = language;
            ParatextGuid = paratextGuid;
            CorpusType = corpusType;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }
        public bool IsRtl { get; set; } = false;
        public string? FontFamily { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Language { get; set; }
        public string? ParatextGuid { get; set; }
        public string CorpusType { get; set; } = Models.CorpusType.Standard.ToString();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; set; }

        public override bool Equals(object? obj) => Equals(obj as CorpusId);
        public bool Equals(CorpusId? other)
        {
            if (other is null) return false;
            if (!IdEquals(other)) return false;
            if (IsRtl != other.IsRtl ||
                FontFamily!= other.FontFamily ||
                Name != other.Name ||
                DisplayName != other.DisplayName ||
                Language != other.Language ||
                ParatextGuid != other.ParatextGuid ||
                CorpusType != other.CorpusType ||
                Created != other.Created ||
                UserId != other.UserId)
            {
                return false;
            }

            return Metadata.SequenceEqual(other.Metadata);
        }
        public override int GetHashCode()
        {
            var mhc = 0;
            foreach (var item in Metadata)
            {
                mhc ^= (item.Key, item.Value).GetHashCode();
            }

            HashCode hash = new();
            hash.Add(Id);
            hash.Add(IsRtl);
            hash.Add(FontFamily);
            hash.Add(Name);
            hash.Add(DisplayName);
            hash.Add(Language);
            hash.Add(ParatextGuid);
            hash.Add(CorpusType);
            hash.Add(Created);
            hash.Add(UserId);
            hash.Add(mhc);
            return hash.ToHashCode();
        }
        public static bool operator ==(CorpusId? e1, CorpusId? e2) => object.Equals(e1, e2);
        public static bool operator !=(CorpusId? e1, CorpusId? e2) => !(e1 == e2);

    }
}
