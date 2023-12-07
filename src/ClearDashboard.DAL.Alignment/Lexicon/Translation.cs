using System.Collections.ObjectModel;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Translation : IEquatable<Translation>
    {
        public TranslationId TranslationId
        {
            get;
#if DEBUG
            set;
#else 
            // RELEASE MODIFIED
            //internal set;
            set;
#endif
        }

        private string? text_;
        public string? Text
        {
            get => text_;
            set
            {
                text_ = value;
                IsDirty = true;
            }
        }

        public string? OriginatedFrom { get; init; }

        public bool IsDirty { get; internal set; } = false;
        public bool IsInDatabase { get => TranslationId.Created is not null; }

        /// <summary>
        /// When set to true, only affects a single Save operation
        /// </summary>
        public bool ExcludeFromSave { get; set; } = false;

        public Translation()
        {
            TranslationId = TranslationId.Create(Guid.NewGuid());
        }
        internal Translation(TranslationId translationId, string text, string? originatedFrom)
        {
            TranslationId = translationId;
            text_ = text;
            OriginatedFrom = originatedFrom;
        }

        internal void PostSave(TranslationId? translationId)
        {
            if (!ExcludeFromSave)
            {
                TranslationId = translationId ?? TranslationId;
                IsDirty = false;
			}

            // When set to true, it only affects a single Save
            ExcludeFromSave = false;
		}

        internal void PostSaveAll(IDictionary<Guid, IId> createdIIdsByGuid)
        {
            createdIIdsByGuid.TryGetValue(TranslationId.Id, out var translationId);
            PostSave((TranslationId?)translationId);
        }

        public override bool Equals(object? obj) => Equals(obj as Translation);
        public bool Equals(Translation? other)
        {
            if (other is null) return false;
            if (!TranslationId.Id.Equals(TranslationId.Id)) return false;

            return true;
        }
        public override int GetHashCode() => TranslationId.Id.GetHashCode();
        public static bool operator ==(Translation? e1, Translation? e2) => Equals(e1, e2);
        public static bool operator !=(Translation? e1, Translation? e2) => !(e1 == e2);
    }
}
