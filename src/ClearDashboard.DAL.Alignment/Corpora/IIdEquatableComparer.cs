using ClearBible.Engine.Utils;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class IIdEquatableComparer : IEqualityComparer<IIdEquatable>
    {
        public bool Equals(IIdEquatable? x, IIdEquatable? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.IdEquals(y);
        }
        public int GetHashCode(IIdEquatable idEquatable) => idEquatable.GetIdHashcode();
    }
}
