using System;
using System.Xml;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Slices;

public abstract class XmlReaderRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
    where TRequest : IRequest<TResponse>
    where TData : new()
{
    protected XmlReader? XmlReader { get; private set; }

    protected XmlReaderRequestHandler(ILogger logger) : base(logger)
    {
        //no-op
    }

    protected TData? LoadXmlAndProcessData(XmlReaderSettings? xmlReaderSettings = null)
    {
        try
        {
            XmlReader = xmlReaderSettings != null ? XmlReader.Create(ResourcePath, xmlReaderSettings): XmlReader.Create(ResourcePath);
            return ProcessData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"An unexpected error occurred while processing the XML file: '{ResourcePath}'");
            throw;
        }
        finally
        {
            XmlReader?.Dispose();
        }
    }
}