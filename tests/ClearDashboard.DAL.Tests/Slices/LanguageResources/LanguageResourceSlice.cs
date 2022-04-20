using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ClearDashboard.DataAccessLayer.Slices;
using ClearDashboard.DataAccessLayer.Slices.ManuscriptVerses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Tests.Slices.LanguageResources
{
    public record GetLanguageResourcesCommand : IRequest<QueryResult<List<string>>>;

    public class GetLanguageResourcesCommandHandler : XmlReaderRequestHandler<GetLanguageResourcesCommand, QueryResult<List<string>>, List<string>>
    {
        public GetLanguageResourcesCommandHandler(ILogger<GetManuscriptVerseByIdHandler> logger) : base(logger)
        {
            //no-op
        }

        protected override string ResourceName { get; set; } = @"xml\\languages.xml";

        public override Task<QueryResult<List<string>>> Handle(GetLanguageResourcesCommand request, CancellationToken cancellationToken)
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
