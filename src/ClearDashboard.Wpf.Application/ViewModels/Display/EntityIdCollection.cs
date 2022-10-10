using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClearBible.Engine.Utils;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class EntityIdCollection: ObservableCollection<IId>
    {
        public EntityIdCollection()
        {
            
        }

        public EntityIdCollection(IEnumerable<IId> ids) : base(ids)
        {
            
        }
    }
}
