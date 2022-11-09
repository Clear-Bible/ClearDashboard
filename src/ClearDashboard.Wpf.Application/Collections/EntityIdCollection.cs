using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using ClearBible.Engine.Utils;

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
