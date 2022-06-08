using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.PINS
{
    public record GetLexiconQuery(ProjectManager ProjectManager) : IRequest<RequestResult<Lexicon>>;

    public class GetLexiconSlice : XmlReaderRequestHandler<GetLexiconQuery,
        RequestResult<Lexicon>, Lexicon>
    {
        private string _projectPath = "";
        private Lexicon _biblicalTermsList = new();

        public GetLexiconSlice(ILogger<PINS.GetLexiconSlice> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<Lexicon>> Handle(GetLexiconQuery request,
            CancellationToken cancellationToken)
        {
            ResourceName = Path.Combine(request.ProjectManager.CurrentDashboardProject.DirectoryPath, "Lexicon.xml");

            var queryResult = ValidateResourcePath(new Lexicon());
            if (queryResult.Success == false)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the TermRenderings.xml file : '{ResourceName}'");
                return Task.FromResult(queryResult);
            }

            try
            {
                queryResult.Data = LoadXmlAndProcessData(null);
            }
            catch (Exception ex)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the '{ResourceName}' database ",
                    ex);
            }

            return Task.FromResult(queryResult);
        }

        protected override Lexicon ProcessData()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ResourceName);
            XmlNodeReader reader = new XmlNodeReader(doc);

            using (reader)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Lexicon));
                try
                {
                    _biblicalTermsList = (Lexicon)serializer.Deserialize(reader);
                }
                catch (Exception e)
                {
                    Logger.LogError("Error in PINS deserialization of TermRenderings.xml: " + e.Message);
                }
            }

            return _biblicalTermsList;
        }
    }
}
