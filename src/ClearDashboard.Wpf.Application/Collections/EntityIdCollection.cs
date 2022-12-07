using Caliburn.Micro;
using ClearBible.Engine.Utils;
using System.Collections.Generic;

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
    }
}
