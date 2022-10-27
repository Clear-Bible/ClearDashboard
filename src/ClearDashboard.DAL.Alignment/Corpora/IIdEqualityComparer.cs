using ClearBible.Engine.Utils;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class IIdEqualityComparer : IEqualityComparer<IId>
    {
        public bool Equals(IId? x, IId? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id.Equals(y.Id);
        }
        public int GetHashCode(IId iid) => iid.Id.GetHashCode();
    }
}
