using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms
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
