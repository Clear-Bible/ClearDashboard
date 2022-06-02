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
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.PINS
{

    public record GetTermRenderingsQuery(string projectPath) : IRequest<RequestResult<TermRenderingsList>>;

    public class GetTermRenderingsSlice : XmlReaderRequestHandler<GetTermRenderingsQuery,
        RequestResult<TermRenderingsList>, TermRenderingsList>
    {
        private string _projectPath = "";
        private TermRenderingsList _termRenderingsList = new();

        public GetTermRenderingsSlice(ILogger<PINS.GetTermRenderingsSlice> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<TermRenderingsList>> Handle(GetTermRenderingsQuery request,
            CancellationToken cancellationToken)
        {
            _projectPath = request.projectPath;

            ResourceName = Path.Combine(_projectPath, "TermRenderings.xml");

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
