using System.Collections.Generic;
using MediatR;
using ParaTextPlugin.Data.Models;



namespace ParaTextPlugin.Data.Features.BiblicalTerms
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
