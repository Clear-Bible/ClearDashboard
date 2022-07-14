using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
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

    public record GetTermRenderingsQuery() : IRequest<RequestResult<TermRenderingsList>>;

    public class GetTermRenderingsQueryHandler : XmlReaderRequestHandler<GetTermRenderingsQuery,
        RequestResult<TermRenderingsList>, TermRenderingsList>
    {
        #nullable disable

        private TermRenderingsList _termRenderingsList = new();
        private readonly ProjectManager _projectManager;


        public GetTermRenderingsQueryHandler(ILogger<GetTermRenderingsQueryHandler> logger, ProjectManager projectManager) : base(logger)
        {
            _projectManager = projectManager;
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<TermRenderingsList>> Handle(GetTermRenderingsQuery request,
            CancellationToken cancellationToken)
        {
            ResourceName = Path.Combine(_projectManager.CurrentDashboardProject.DirectoryPath, "TermRenderings.xml");
            var queryResult = ValidateResourcePath(new TermRenderingsList());
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

        protected override TermRenderingsList ProcessData()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ResourceName);
            XmlNodeReader reader = new XmlNodeReader(doc);

            using (reader)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TermRenderingsList));
                try
                {
                    _termRenderingsList = (TermRenderingsList)serializer.Deserialize(reader);
                }
                catch (Exception e)
                {
                    Logger.LogError("Error in PINS deserialization of TermRenderings.xml: " + e.Message);
                }
            }

            return _termRenderingsList;
        }
    }
}
