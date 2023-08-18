using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
    public  class LoadSemanticDictionaryLookupSlice
    {
        public record LoadSemanticDictionaryLookupQuery() : IRequest<RequestResult<List<SemanticDomainLookup>>>;


        public class LoadSemanticDictionaryLookupHandler : CsvReaderRequestHandler<LoadSemanticDictionaryLookupQuery,
            RequestResult<List<SemanticDomainLookup>>, List<SemanticDomainLookup>>
        {
            private readonly ILogger<LoadSemanticDictionaryLookupHandler> _logger;

            public LoadSemanticDictionaryLookupHandler(ILogger<LoadSemanticDictionaryLookupHandler> logger) :
                base(logger)
            {
                _logger = logger;
                //no-op
            }


            protected override string ResourceName { get; set; } = "";

            //public override Task<RequestResult<List<MarbleResource>>> Handle(GetWhatIsThisWordByBcvQuery request,
            //    CancellationToken cancellationToken)
            //{
            //    _bcv = request.bcv;
            //    _languageCode = request.languageCode;

            //    ResourceName = GetFilenameFromMarbleBook(_bcv.BookNum);
            //    ResourceName = @"marble-indexes-full\MARBLELinks-" + ResourceName + ".XML";

            //    var queryResult = ValidateResourcePath(new List<MarbleResource>());
            //    if (queryResult.Success == false)
            //    {
            //        LogAndSetUnsuccessfulResult(ref queryResult,
            //            $"An unexpected error occurred while querying the MARBLE databases for data with verseId : '{_bcv.BBBCCCVVV}'");
            //        return Task.FromResult(queryResult);
            //    }

            //    try
            //    {
            //        queryResult.Data = LoadXmlAndProcessData(null);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogAndSetUnsuccessfulResult(ref queryResult,
            //            $"An unexpected error occurred while querying the '{ResourceName}' database for data with verseId : '{_bcv.BBBCCCVVV}'",
            //            ex);
            //    }

            //    return Task.FromResult(queryResult);
            //}

            public override Task<RequestResult<List<SemanticDomainLookup>>> Handle(
                LoadSemanticDictionaryLookupQuery request, CancellationToken cancellationToken)
            {
                var fullList = new List<SemanticDomainLookup>();

                // DO HEBREW LOOKUP
                ResourceName = Path.Combine(Environment.CurrentDirectory, @"Resources\SDBH\lemmaLU.csv");

                var queryResult = ValidateResourcePath(new List<SemanticDomainLookup>());
                if (queryResult.Success == false)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the MARBLE CSV Lookup databases : '{ResourceName}'");
                    return Task.FromResult(queryResult);
                }

                try
                {
                    queryResult.Data = LoadCsvAndProcessData(ResourceName);
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

                queryResult = ValidateResourcePath(new List<SemanticDomainLookup>());
                if (queryResult.Success == false)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the MARBLE CSV Lookup databases : '{ResourceName}'");
                    return Task.FromResult(queryResult);
                }

                try
                {
                    queryResult.Data = LoadCsvAndProcessData(ResourceName);
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

            protected override List<SemanticDomainLookup> ProcessData()
            {
                //return GetLemmaListFromMarbleIndexes(ResourcePath, _bcv, _languageCode);


                List<SemanticDomainLookup> lookup = new();

                var lines = File.ReadAllLines(ResourceName);
                foreach (string item in lines)
                {
                    var values = item.Split(',');
                    if (values.Count() > 0)
                    {
                        lookup.Add(new SemanticDomainLookup
                        {
                            Word = values[0],
                            FileName = values[1],
                            LineNum = int.Parse(values[2])
                        });
                    }
                }

                return lookup;
            }
        }


    }
}
