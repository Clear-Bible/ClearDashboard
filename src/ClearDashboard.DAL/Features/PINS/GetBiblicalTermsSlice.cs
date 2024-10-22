﻿using ClearDashboard.DAL.CQRS;
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
    public enum BTtype
    {
        allBT,
        BT
    }

    public record GetBiblicalTermsQuery(string ParatextInstallPath, BTtype BTtype) : IRequest<RequestResult<BiblicalTermsList>>;

    public class GetBiblicalTermsQueryHandler : XmlReaderRequestHandler<GetBiblicalTermsQuery,
        RequestResult<BiblicalTermsList>, BiblicalTermsList>
    {
        private BiblicalTermsList? _biblicalTermsList = new();

        public GetBiblicalTermsQueryHandler(ILogger<PINS.GetTermRenderingsQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; } = "";

        public override Task<RequestResult<BiblicalTermsList>> Handle(GetBiblicalTermsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.BTtype == BTtype.BT)
            {
                ResourceName = Path.Combine(request.ParatextInstallPath, @"Terms\Lists\BiblicalTerms.xml");
            }
            else
            {
                ResourceName = Path.Combine(request.ParatextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml");
            }

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
                    _biblicalTermsList = (BiblicalTermsList?)serializer.Deserialize(reader);
                }
                catch (Exception e)
                {
                    Logger.LogError("Error in PINS deserialization of TermRenderings.xml: " + e.Message);
                }
            }

            return _biblicalTermsList!;
        }
    }
}
