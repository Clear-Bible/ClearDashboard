using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Features.PINS
{
    public record GetLexiconQuery() : IRequest<RequestResult<Models.Lexicon>>;

    public class GetLexiconQueryHandler : XmlReaderRequestHandler<GetLexiconQuery,
        RequestResult<Models.Lexicon>, Models.Lexicon>
    {
        #nullable disable

        private Models.Lexicon _biblicalTermsList = new();
        private readonly ProjectManager _projectManager;

        public GetLexiconQueryHandler(ILogger<GetLexiconQueryHandler> logger, ProjectManager ProjectManager) : base(logger)
        {
            _projectManager = ProjectManager;
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<Models.Lexicon>> Handle(GetLexiconQuery request,
            CancellationToken cancellationToken)
        {
            if (_projectManager.HasCurrentParatextProject == false)
            {
                var ret = new RequestResult<Models.Lexicon>();
                ret.Success = false;
                ret.Message = "No CurrentParatextProject - Plugin is probably not running";
                return Task.FromResult(ret);
            }

            ResourceName = Path.Combine(_projectManager.CurrentParatextProject.DirectoryPath, "Lexicon.xml");

            var queryResult = ValidateResourcePath(new Models.Lexicon());
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

        protected override Models.Lexicon ProcessData()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ResourceName);
            XmlNodeReader reader = new XmlNodeReader(doc);

            using (reader)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Models.Lexicon));
                try
                {
                    _biblicalTermsList = (Models.Lexicon)serializer.Deserialize(reader);
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
