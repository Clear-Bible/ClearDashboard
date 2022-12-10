using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ClearDashboard.DataAccessLayer.Features.MarbleDataRequests.LoadSemanticDictionaryLookupSlice;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
    public class LoadSemanticDictionaryGlossesSlice
    {
        public record LoadSemanticDictionaryGlossesQuery() : IRequest<RequestResult<List<SemanticGlossesLookup>>>;

        public class LoadSemanticDictionaryGlossesHandler : XmlReaderRequestHandler<LoadSemanticDictionaryGlossesQuery,
     RequestResult<List<SemanticGlossesLookup>>, List<SemanticGlossesLookup>>
        {
            private readonly ILogger<LoadSemanticDictionaryGlossesHandler> _logger;

            public LoadSemanticDictionaryGlossesHandler(ILogger<LoadSemanticDictionaryGlossesHandler> logger) :
                base(logger)
            {
                _logger = logger;
                //no-op
            }


            protected override string ResourceName { get; set; } = "";


            public override Task<RequestResult<List<SemanticGlossesLookup>>> Handle(
                LoadSemanticDictionaryGlossesQuery request, CancellationToken cancellationToken)
            {
                var fullList = new List<SemanticGlossesLookup>();

                // DO HEBREW LOOKUP
                ResourceName = Path.Combine(Environment.CurrentDirectory, @"Resources\SDBH\lemmaLU.csv");

                var queryResult = ValidateResourcePath(new List<SemanticGlossesLookup>());
                if (queryResult.Success == false)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the MARBLE databases : '{ResourceName}'");
                    return Task.FromResult(queryResult);
                }

                try
                {
                    queryResult.Data = FileLoadXmlAndProcessData();
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the '{ResourceName}' database'",
                        ex);
                }

                fullList.AddRange(queryResult.Data);


                // DO GREEK LOOKUP
                ResourceName = Path.Combine(Environment.CurrentDirectory, @"Resources\SDBG\lemmaLU.csv");

                queryResult = ValidateResourcePath(new List<SemanticGlossesLookup>());
                if (queryResult.Success == false)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the MARBLE CSV Lookup databases : '{ResourceName}'");
                    return Task.FromResult(queryResult);
                }

                try
                {
                    queryResult.Data = FileLoadXmlAndProcessData();
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the '{ResourceName}' database'",
                        ex);
                }

                // merge hebrew onto the greek
                queryResult.Data.AddRange(fullList);

                return Task.FromResult(queryResult);
            }

            protected override List<SemanticGlossesLookup> ProcessData()
            {
                //return GetLemmaListFromMarbleIndexes(ResourcePath, _bcv, _languageCode);


                List<SemanticGlossesLookup> lookup = new();

                //var lines = File.ReadAllLines(ResourceName);
                //foreach (string item in lines)
                //{
                //    var values = item.Split(',');
                //    if (values.Count() > 0)
                //    {
                //        lookup.Add(new SemanticGlossesLookup
                //        {
                //            Word = values[0],
                //            FileName = values[1],
                //            LineNum = int.Parse(values[2])
                //        });
                //    }
                //}

                return lookup;
            }
        }


    }
}
