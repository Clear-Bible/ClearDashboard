using Models = ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections;
using ClearDashboard.DataAccessLayer.Models;
using System.Linq.Expressions;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class AlignmentModelExtensions
    {
        public static IEnumerable<Models.Alignment> WhereAlignmentTypesFilter(this IEnumerable<Models.Alignment> alignments, AlignmentTypes alignmentTypes)
        {
            List<(AlignmentOriginatedFrom originatedFrom, AlignmentVerification verification)> orCombinations = new();

            if ((alignmentTypes & AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included) != AlignmentTypes.None ||
                (alignmentTypes & AlignmentTypes.FromAlignmentModel_Unverified_All) != AlignmentTypes.None)
            {
                orCombinations.Add((AlignmentOriginatedFrom.FromAlignmentModel, AlignmentVerification.Unverified));
            }

            var alignmentTypeValues = Enum.GetValues(typeof(AlignmentTypes))
                .Cast<AlignmentTypes>()
                .Where(e => alignmentTypes.HasFlag(e))
                .Where(e => e != AlignmentTypes.FromAlignmentModel_Unverified_All)
                .Where(e => e != AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included)
                .Where(e => e != AlignmentTypes.None);

            foreach ( var value in alignmentTypeValues )
            {
                var parts = value.ToString().Split('_');
                if (parts.Length != 2)
                    throw new NotSupportedException($"Encountered AlignmentTypes enum value {value} from which OriginatedFrom and Verification cannot be automatically extracted");

                var alignmentOriginatedFrom = Enum.Parse<AlignmentOriginatedFrom>(parts[0]);
                var alignmentVerification = Enum.Parse<AlignmentVerification>(parts[1]);

                orCombinations.Add((alignmentOriginatedFrom, alignmentVerification));
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(Models.Alignment), "e");
            BinaryExpression? combinedOrExpressions = null;

            if (orCombinations.Any())
            {
                MemberExpression originatedFromProperty = Expression.PropertyOrField(parameterExpression, nameof(Models.Alignment.AlignmentOriginatedFrom));
                MemberExpression verificationProperty = Expression.PropertyOrField(parameterExpression, nameof(Models.Alignment.AlignmentVerification));

                foreach (var (originatedFrom, verification) in orCombinations)
                {
                    BinaryExpression originatedAndVerificationEqual = Expression.AndAlso(
                        Expression.Equal(originatedFromProperty, Expression.Constant(originatedFrom, typeof(AlignmentOriginatedFrom))),
                        Expression.Equal(verificationProperty, Expression.Constant(verification, typeof(AlignmentVerification))));

                    if (combinedOrExpressions is null)
                    {
                        combinedOrExpressions = originatedAndVerificationEqual;
                    }
                    else
                    {
                        combinedOrExpressions = Expression.OrElse(combinedOrExpressions, originatedAndVerificationEqual);
                    }
                }
            }

            var filteredAlignments = alignments;
            if (combinedOrExpressions is not null)
            {
                var whereExpression = Expression.Lambda<Func<Models.Alignment, bool>>(combinedOrExpressions, parameterExpression);
                var compiledWhere = whereExpression.Compile();

                filteredAlignments = filteredAlignments.Where(compiledWhere);
            }

            if ((alignmentTypes & AlignmentTypes.FromAlignmentModel_Unverified_All) == AlignmentTypes.None &&
                (alignmentTypes & AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included) != AlignmentTypes.None)
            {
                filteredAlignments = filteredAlignments.WhereAssignedOrFromAlignmentModel();
            }

            return filteredAlignments;
        }

        public static AlignmentTypes ToAlignmentType(this Models.Alignment alignment, AlignmentTypes alignmentTypesUsedInQuery)
        {
            if (alignment.AlignmentOriginatedFrom == AlignmentOriginatedFrom.FromAlignmentModel && alignment.AlignmentVerification == AlignmentVerification.Unverified)
            {
                return ((alignmentTypesUsedInQuery & AlignmentTypes.FromAlignmentModel_Unverified_All) != AlignmentTypes.None)
                    ? AlignmentTypes.FromAlignmentModel_Unverified_All
                    : AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included;
            }

            string originatedFromAndVerification = alignment.AlignmentOriginatedFrom.ToString() + "_" + alignment.AlignmentVerification.ToString();
            if (Enum.TryParse(originatedFromAndVerification, true, out AlignmentTypes alignmentType))
            {
                return alignmentType;
            }

            throw new NotImplementedException($"No AlignmentTypes value available for OriginatedFrom: {alignment.AlignmentOriginatedFrom} and Verification: {alignment.AlignmentVerification} combination");
        }

        public static IEnumerable<Models.Alignment> WhereAssignedOrFromAlignmentModel(this IEnumerable<Models.Alignment> alignments)
        {
            var filteredAlignments = alignments
                    .Where(al => al.AlignmentOriginatedFrom == Models.AlignmentOriginatedFrom.Assigned);

            // Only include "FromAlignmentModel" alignments that touch tokens that
            // are not touched by any "Assigned" alignments:
            var filteredSetSourceTokenIdFilterer = filteredAlignments.ToHashSet(new AlignmentTokenIdEqualityComparer(true));
            var filteredSetTargetTokenIdFilterer = filteredAlignments.ToHashSet(new AlignmentTokenIdEqualityComparer(false));

            var nonAssignedFromAlignmentModelAlignments = alignments
                .Where(al => al.AlignmentOriginatedFrom == Models.AlignmentOriginatedFrom.FromAlignmentModel)
                .Where(v => !filteredSetSourceTokenIdFilterer.Contains(v))
                .Where(v => !filteredSetTargetTokenIdFilterer.Contains(v));

            filteredAlignments = filteredAlignments.Union(nonAssignedFromAlignmentModelAlignments);

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
