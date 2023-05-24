using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.CQRS;
using Models = ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class AlignmentModelExtensions
    {
        public static IEnumerable<Models.Alignment> FilterByAlignmentMode(this IEnumerable<Models.Alignment> alignments, ManualAutoAlignmentMode manualAutoAlignmentMode)
        {
            var filteredAlignments = alignments;

            if (manualAutoAlignmentMode == ManualAutoAlignmentMode.ManualOnly ||
                manualAutoAlignmentMode == ManualAutoAlignmentMode.ManualAndOnlyNonManualAuto)
            {
                filteredAlignments = alignments
                    .Where(al => al.AlignmentOriginatedFrom == Models.AlignmentOriginatedFrom.Assigned);

                if (manualAutoAlignmentMode == ManualAutoAlignmentMode.ManualAndOnlyNonManualAuto)
                {
                    // Only include "FromAlignmentModel" alignments that touch tokens that
                    // are not touched by any "Assigned" alignments:
                    var filteredSetSourceTokenIdFilterer = filteredAlignments.ToHashSet(new AlignmentTokenIdEqualityComparer(true));
                    var filteredSetTargetTokenIdFilterer = filteredAlignments.ToHashSet(new AlignmentTokenIdEqualityComparer(false));

                    var nonManualAutoAlignments = alignments
                        .Where(al => al.AlignmentOriginatedFrom == Models.AlignmentOriginatedFrom.FromAlignmentModel)
                        .Where(v => !filteredSetSourceTokenIdFilterer.Contains(v))
                        .Where(v => !filteredSetTargetTokenIdFilterer.Contains(v));

                    filteredAlignments = filteredAlignments.Union(nonManualAutoAlignments);
                }
            }

            return filteredAlignments;
        }
        private class AlignmentTokenIdEqualityComparer : IEqualityComparer<Models.Alignment>
        {
            private readonly bool _compareSource;
            public AlignmentTokenIdEqualityComparer(bool compareSource)
            {
                _compareSource = compareSource;
            }

            public bool Equals(Models.Alignment? x, Models.Alignment? y)
            {
                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the Alignments' token Ids are equal.
                return _compareSource
                    ? x.SourceTokenComponentId.Equals(y.SourceTokenComponentId)
                    : x.TargetTokenComponentId.Equals(y.TargetTokenComponentId);
            }

            public int GetHashCode([DisallowNull] Models.Alignment obj) => _compareSource
                ? obj.SourceTokenComponentId.GetHashCode()
                : obj.TargetTokenComponentId.GetHashCode();
        }
    }
}
