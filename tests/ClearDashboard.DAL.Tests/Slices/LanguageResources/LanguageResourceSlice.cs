using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ClearDashboard.DAL.Tests.Slices.LanguageResources
{
    public record GetLanguageResourcesQuery : IRequest<RequestResult<List<string>>>;

    public class GetLanguageResourcesQueryHandler : XmlReaderRequestHandler<GetLanguageResourcesQuery, RequestResult<List<string>>, List<string>>
    {
        public GetLanguageResourcesQueryHandler(ILogger<GetLanguageResourcesQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        protected override string ResourceName { get; set; } = @"xml\\languages.xml";

        public override Task<RequestResult<List<string>>> Handle(GetLanguageResourcesQuery request, CancellationToken cancellationToken)
        {
            var queryResult = ValidateResourcePath(new List<string>());
            if (queryResult.Success)
            {
                try
                {
                    queryResult.Data = LoadXmlAndProcessData();
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while loading and processing the '{ResourceName}' file.'",
                        ex);
                }
            }

            return Task.FromResult(queryResult);
        }

        protected override List<string> ProcessData()
        {
            var list = new List<string>();
            if (XmlReader != null)
            {
                while (XmlReader.Read())
                {
                    if (XmlReader.NodeType == XmlNodeType.Element && XmlReader.Name == "Language")
                    {
                        var name = XmlReader.GetAttribute("Name");
                        if (!string.IsNullOrEmpty(name))
                        {
                            list.Add(name);
                        }
                       
                    }
                }
            }
            return list;
        }
    }
}
