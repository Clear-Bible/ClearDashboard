using System;
using System.Collections.Generic;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    /// <summary>
    /// A specialization of <see cref="EntityIdCollection"/> where the IDs are full entity IDs (e.g. <see cref="TokenId"/>) rather than just <see cref="Guid"/>.
    /// </summary>
    public class FullEntityIdCollection: EntityIdCollection
    {
        public FullEntityIdCollection()
        {
            
        }

        public FullEntityIdCollection(IEnumerable<IId> ids) : base(ids)
        {
            
        }
    }
}
