using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Paratext;
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
    public record GetSpellingStatusQuery : IRequest<RequestResult<SpellingStatus>>;

    public record GetSpellingStatusQuery() : IRequest<RequestResult<SpellingStatus>>;

    public class GetSpellingStatusQueryHandler : XmlReaderRequestHandler<GetSpellingStatusQuery,
        RequestResult<SpellingStatus>, SpellingStatus>
    {
        private SpellingStatus _biblicalTermsList = new();
        private readonly ProjectManager _projectManager;

        public GetSpellingStatusQueryHandler(ILogger<PINS.GetSpellingStatusQueryHandler> logger, ProjectManager projectManager) : base(logger)
        {
            _projectManager = projectManager;
        }
        protected override string ResourceName { get; set; } = "";
        public override Task<RequestResult<SpellingStatus>> Handle(GetSpellingStatusQuery request,
            CancellationToken cancellationToken)
        {
            ResourceName = Path.Combine(_projectManager.CurrentDashboardProject.DirectoryPath,
                "SpellingStatus.xml");
            var queryResult = ValidateResourcePath(new SpellingStatus());
            if (queryResult.Success == false)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the TermRenderings.xml file : '{ResourceName}'");
                return Task.FromResult(queryResult);
            }
            try
            {
                queryResult.Data = LoadXmlAndProcessData();
            }
            catch (Exception ex)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the '{ResourceName}' database ",
                    ex);
            }
            return Task.FromResult(queryResult);
        }
        protected override SpellingStatus ProcessData()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ResourceName);
            XmlNodeReader reader = new XmlNodeReader(doc);
            using (reader)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SpellingStatus));
                try
                {
                    _biblicalTermsList = (SpellingStatus)serializer.Deserialize(reader);
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

