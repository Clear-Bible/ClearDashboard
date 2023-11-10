using System;
using System.Collections.ObjectModel;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.SqlServer.Server;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Form : IEquatable<Form>
    {
        public FormId FormId
        {
            get;
#if DEBUG
            internal set;
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

        public bool IsDirty { get; internal set; } = false;
        public bool IsInDatabase { get => FormId.Created is not null; }

        /// <summary>
        /// When set to true, only affects a single Save operation
        /// </summary>
        public bool ExcludeFromSave { get; set; } = false;

        public Form()
        {
            FormId = FormId.Create(Guid.NewGuid());
        }
        internal Form(FormId formId, string text)
        {
            FormId = formId;
            text_ = text;
        }

        internal void PostSave(FormId? formId)
        {
            if (!ExcludeFromSave)
            {
                FormId = formId ?? FormId;
                IsDirty = false;
            }

            // When set to true, it only affects a single Save
            ExcludeFromSave = false;
        }

        internal void PostSaveAll(IDictionary<Guid, IId> createdIIdsByGuid)
        {
            createdIIdsByGuid.TryGetValue(FormId.Id, out var formId);
            PostSave((FormId?)formId);
        }

        public override bool Equals(object? obj) => Equals(obj as Form);
        public bool Equals(Form? other)
        {
            if (other is null) return false;
            if (!FormId.Id.Equals(other.FormId.Id)) return false;

            return true;
        }
        public override int GetHashCode() => FormId.Id.GetHashCode();
        public static bool operator ==(Form? e1, Form? e2) => Equals(e1, e2);
        public static bool operator !=(Form? e1, Form? e2) => !(e1 == e2);
    }
}