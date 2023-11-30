using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Data;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.DAL.Alignment.Translation;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace ClearDashboard.Collaboration.Model;

public class NoteModelRef<T> : NoteModelRef, IEquatable<NoteModelRef<T>> where T : ModelRef<T>
{
    public NoteModelRef(Guid noteDomainEntityAssociationId, Guid noteId, T modelRef): base(noteDomainEntityAssociationId, noteId, modelRef)
    {
    }

    public override IModelDifference<NoteModelRef> GetModelDifference(NoteModelRef other)
    {
        if (other is NoteModelRef<T> @ref) { return GetPropertyDifferences(@ref).ToModelDifference<NoteModelRef>(typeof(NoteModelRef), GetId()); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }

    protected IEnumerable<PropertyDifference> GetPropertyDifferences(NoteModelRef<T> other)
    {
        if (!this.GetId().Equals(other.GetId()))
        {
            throw new Exception($"Invalid comparison between {this.GetType().ShortDisplayName()} instances having different Ids '{GetId()}' vs '{other.GetId()}'");
        }

        var propertyDifferences = new List<PropertyDifference>();

        if (!NoteId.Equals(other.NoteId))
        {
            propertyDifferences.Add(new PropertyDifference(nameof(NoteId), new ValueDifference<Guid>(NoteId, other.NoteId)));
        }
        if (!ModelRef.Equals(other.ModelRef))
        {
            propertyDifferences.Add(new PropertyDifference(nameof(ModelRef), ((T)ModelRef!).GetModelDifference((T)other.ModelRef!)));
        }

        return propertyDifferences;
    }

    public override void ApplyPropertyDifference(PropertyDifference propertyDifference)
    {
        var propertyName = propertyDifference.PropertyName;

        if (nameof(NoteDomainEntityAssociationId) == propertyName) { NoteDomainEntityAssociationId = (Guid)((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!; }
        if (nameof(NoteId) == propertyName) { NoteId = (Guid)((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!; }
        if (nameof(ModelRef) == propertyName)
        {
            foreach (var pd in ((IModelDifference)propertyDifference.PropertyValueDifference).PropertyDifferences)
            {
                ModelRef.ApplyPropertyDifference(pd);
            }
        }
    }

    // ======================================================================================
    // FIXME:  Keep these next properties if we decide to make NoteModelRef an IModelSnapshot
    // (maybe something to do with Merge)
    //[JsonIgnore]
    //public override IReadOnlyDictionary<string, Type> PropertyTypes => new Dictionary<string, Type>() {
    //    { nameof(NoteDomainEntityAssociationId), typeof(Guid) },
    //    { nameof(NoteId), typeof(Guid) },
    //    { nameof(ModelRef), typeof(T) }
    //};
    // ======================================================================================

    public override bool Equals(object? obj) => Equals(obj as NoteModelRef<T>);
    public override bool Equals(NoteModelRef? other) => Equals(other as NoteModelRef<T>);
    public bool Equals(NoteModelRef<T>? other)
    {
        if (other == null) return false;
        return
            this.NoteDomainEntityAssociationId == other.NoteDomainEntityAssociationId &&
            this.NoteId == other.NoteId &&
            this.ModelRef == other.ModelRef;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(NoteDomainEntityAssociationId, NoteId, ModelRef?.GetHashCode());
    }

    public static bool operator ==(NoteModelRef<T>? e1, NoteModelRef<T>? e2) => object.Equals(e1, e2);
    public static bool operator !=(NoteModelRef<T>? e1, NoteModelRef<T>? e2) => !(e1 == e2);
}

public abstract class NoteModelRef : IModelSnapshot<Models.NoteDomainEntityAssociation>, IModelDistinguishable<NoteModelRef>, IEquatable<NoteModelRef>
{
    public NoteModelRef(Guid noteDomainEntityAssociationId, Guid noteId, ModelRef modelRef)
    {
        NoteDomainEntityAssociationId = noteDomainEntityAssociationId;
        NoteId = noteId;
        ModelRef = modelRef;
    }

    public Guid NoteDomainEntityAssociationId { get; set; } = Guid.Empty;
    public Guid NoteId { get; set; } = Guid.Empty;
    public ModelRef ModelRef { get; private set; }

    // ======================================================================================
    // FIXME:  Keep these next properties if we decide to make NoteModelRef an IModelSnapshot
    // (maybe something to do with Merge)
    [JsonIgnore]
    public Type EntityType => typeof(Models.NoteDomainEntityAssociation);

    [JsonIgnore]
    public string IdentityKey => nameof(Models.NoteDomainEntityAssociation.Id);

    [JsonIgnore]
    public IReadOnlyDictionary<string, IEnumerable<IModelDistinguishable>> Children =>
        new Dictionary<string, IEnumerable<IModelDistinguishable>>().AsReadOnly();

    [JsonIgnore]
    public IReadOnlyDictionary<string, string>? AddedPropertyTypeNames => null;

    // ======================================================================================

    public abstract bool Equals(NoteModelRef? other);
    public override string ToString()
    {
        return NoteDomainEntityAssociationId.ToString();
    }

    public object GetId() => NoteDomainEntityAssociationId;
    public string GetComparableId() => NoteDomainEntityAssociationId.ToString();

    [JsonIgnore]
    public IReadOnlyDictionary<string, object?> PropertyValues => new Dictionary<string, object?>() {
        { nameof(NoteModelRef.NoteDomainEntityAssociationId), NoteDomainEntityAssociationId },
        { nameof(NoteModelRef.NoteId), NoteId },
        { nameof(NoteModelRef.ModelRef), ModelRef }
    }.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyDictionary<string, Type> PropertyTypes => new Dictionary<string, Type>()
    {
        { nameof(NoteModelRef.NoteDomainEntityAssociationId), typeof(Guid) },
        { nameof(NoteModelRef.NoteId), typeof(Guid) },
        { nameof(NoteModelRef.ModelRef), typeof(ModelRef) }
    }.AsReadOnly();

    public bool TryGetPropertyValue(string key, out object? value)
    {
        bool found = false;
        value = null;

        switch (key)
        {
            case nameof(NoteDomainEntityAssociationId):
                value = this.NoteDomainEntityAssociationId;
                found = true;
                break;

            case nameof(NoteId):
                value = this.NoteId;
                found = true;
                break;

            case nameof(ModelRef):
                value = this.ModelRef;
                found = true;
                break;
        }

        return found;
    }

    public abstract IModelDifference<NoteModelRef> GetModelDifference(NoteModelRef other);
    public abstract void ApplyPropertyDifference(PropertyDifference propertyDifference);

    public IModelDifference GetModelDifference(object other)
    {
        if (other is NoteModelRef @ref) { return this.GetModelDifference(@ref); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }

    public IModelDifference<IModelSnapshot> GetModelDifference(IModelSnapshot other)
    {
        if (other is NoteModelRef @ref) { return GetModelDifference(@ref); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }

    public IModelDifference<IModelSnapshot<Models.NoteDomainEntityAssociation>> GetModelDifference(IModelSnapshot<Models.NoteDomainEntityAssociation> other)
    {
        if (other is NoteModelRef @ref) { return GetModelDifference(@ref); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }
}