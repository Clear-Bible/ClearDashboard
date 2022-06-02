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

    public record GetBiblicalTermsQuery(string xmlPath) : IRequest<RequestResult<BiblicalTermsList>>;

    public class GetBiblicalTermsSlice : XmlReaderRequestHandler<GetBiblicalTermsQuery,
        RequestResult<BiblicalTermsList>, BiblicalTermsList>
    {
        private string _projectPath = "";
        private BiblicalTermsList _biblicalTermsList = new();

        public GetBiblicalTermsSlice(ILogger<PINS.GetTermRenderingsSlice> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<BiblicalTermsList>> Handle(GetBiblicalTermsQuery request,
            CancellationToken cancellationToken)
        {
            ResourceName = request.xmlPath;

            var queryResult = ValidateResourcePath(new BiblicalTermsList());
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

        protected override BiblicalTermsList ProcessData()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ResourceName);
            XmlNodeReader reader = new XmlNodeReader(doc);

            using (reader)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
                try
                {
                    _biblicalTermsList = (BiblicalTermsList)serializer.Deserialize(reader);
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
