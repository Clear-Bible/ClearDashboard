using ClearBible.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features
{
    internal class IdentifiableEntityComparer : IEqualityComparer<Models.IdentifiableEntity>
    {
        public bool Equals(Models.IdentifiableEntity? x, Models.IdentifiableEntity? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode([DisallowNull] Models.IdentifiableEntity obj) => obj.Id.GetHashCode();
    }
}