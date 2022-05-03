using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.Data.Models;
using MediatR;

namespace ClearDashboard.ParatextPlugin.Data.Features.BiblicalTerms
{
    public enum BiblicalTermsType
    {
        Project, 
        All
    }

    public record GetBiblicalTermsByTypeQuery(BiblicalTermsType BiblicalTermsType) : IRequest<QueryResult<List<BiblicalTermsData>>>
    {
        public BiblicalTermsType BiblicalTermsType { get; } = BiblicalTermsType;
    }

  
}
