using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView
{
    // Needed as type to find corresponding viewmodel by convention since all derivatives share the same view model implementation.
    public class VerseAwareEnhancedViewItemMetadatum : EnhancedViewItemMetadatum
    {
        public override Type GetEnhancedViewItemMetadatumType()
        {
            return typeof(VerseAwareEnhancedViewItemMetadatum);
        }
    }
}
