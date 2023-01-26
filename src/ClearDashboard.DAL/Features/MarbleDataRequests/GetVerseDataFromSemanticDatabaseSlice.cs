using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{

    public record GetVerseDataFromSemanticDatabaseQuery
        (BookChapterVerseViewModel bcv) : IRequest<RequestResult<List<LexicalLink>>>;

    public class GetVerseDataFromSemanticDatabaseHandler : XmlReaderRequestHandler<GetVerseDataFromSemanticDatabaseQuery
        ,
        RequestResult<List<LexicalLink>>, List<LexicalLink>>
    {
        private readonly ILogger<GetVerseDataFromSemanticDatabaseHandler> _logger;
        private BookChapterVerseViewModel _bcv;
        private List<LexicalLink> _lexicalLinks;

        public GetVerseDataFromSemanticDatabaseHandler(ILogger<GetVerseDataFromSemanticDatabaseHandler> logger) :
            base(logger)
        {
            _logger = logger;
            //no-op
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<List<LexicalLink>>> Handle(GetVerseDataFromSemanticDatabaseQuery request,
            CancellationToken cancellationToken)
        {
            _bcv = request.bcv;

            ResourceName = Helpers.GetFilenameFromMarbleBook(_bcv.BookNum);
            ResourceName = @"marble-indexes-full\MARBLELinks-" + ResourceName + ".XML";

            var queryResult = ValidateResourcePath(new List<LexicalLink>());
            if (queryResult.Success == false)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the MARBLE databases for data with verseId : '{_bcv.BBBCCCVVV}'");
                return Task.FromResult(queryResult);
            }

            try
            {
                queryResult.Data = FileLoadXmlAndProcessData();
            }
            catch (Exception ex)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the '{ResourceName}' database for data with verseId : '{_bcv.BBBCCCVVV}'",
                    ex);
            }

            return Task.FromResult(queryResult);
        }

        protected override List<LexicalLink> ProcessData()
        {
            var result = ResourceElements.Elements("MARBLELink")
                .Where(x => x.Attribute("Id").Value.StartsWith(_bcv.BBBCCCVVV))
                .ToList();

            List<string> lexicalLinks = new();
            foreach (var element in result)
            {
                var links = element.Elements("LexicalLinks").Elements("LexicalLink").ToList();
                if (links.Count() > 0)
                {
                    lexicalLinks.Add(links[0].Value);
                }
            }

            _lexicalLinks = new();
            foreach (var link in lexicalLinks)
            {
                var split = link.Split(':');

                if (split.Length == 3)
                {
                    DatabaseType dbType = DatabaseType.SDBH;
                    if (split[0] != "SDBH")
                    {
                        dbType = DatabaseType.SDBG;

                        _lexicalLinks.Add(new LexicalLink
                        {
                            Database = dbType,
                            WordSenseId = split[2],
                            Word = split[1]
                        });
                    }
                    else
                    {
                        _lexicalLinks.Add(new LexicalLink
                        {
                            Database = dbType,
                            WordSenseId = split[2],
                            Word = split[1],
                            IsRtl = true
                        });
                    }
                }
            }

            return _lexicalLinks;
        }

    }
}
