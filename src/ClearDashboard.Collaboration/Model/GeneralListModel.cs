using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using System.Linq;
using ClearDashboard.Collaboration.DifferenceModel;

namespace ClearDashboard.Collaboration.Model;

public class GeneralListModel<T> : List<T>, IEquatable<GeneralListModel<T>>, IListModelDistinguishable<T> where T : notnull
{
    public GeneralListModel() : base()
    {
    }

    public GeneralListModel(IEnumerable<T> collection) : base(collection)
    {
    }

    public override bool Equals(object? obj) => Equals(obj as GeneralListModel<T>);
    public bool Equals(GeneralListModel<T>? other)
    {
        if (other == null) return false;
        return this.SequenceEqual(other);
    }

    public override int GetHashCode()
    {
        return this.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()));
    }

    public IListDifference GetListDifference(IEnumerable other)
    {
        if (other is IEnumerable<T>) { return this.GetListDifference((IEnumerable<T>)other); }
        throw new Exception($"Invalid list model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }

    public IListDifference<T> GetListDifference(IEnumerable<T> other)
    {
        // Calls extension method:
        var listDifference = this.GetListDifference<T>(other);
        return listDifference;
    }

    public static bool operator ==(GeneralListModel<T>? e1, GeneralListModel<T>? e2) => object.Equals(e1, e2);
    public static bool operator !=(GeneralListModel<T>? e1, GeneralListModel<T>? e2) => !(e1 == e2);
}

