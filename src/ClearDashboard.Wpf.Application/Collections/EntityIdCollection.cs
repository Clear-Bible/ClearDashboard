using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Utils;
using System.Collections.Generic;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class EntityIdCollection : BindableCollection<IId>
    {
        public EntityIdCollection()
        {

        }

        public EntityIdCollection(IEnumerable<IId> ids) : base(ids)
        {

        }


        public bool ContainsId(IId entityId)
        {
            return this.Contains(entityId, new IIdEqualityComparer());
        }
    }
}
