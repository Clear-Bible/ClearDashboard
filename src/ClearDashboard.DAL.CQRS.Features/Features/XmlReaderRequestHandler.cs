using System.Xml;
using System.Xml.Linq;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features;

public abstract class XmlReaderRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
    where TRequest : IRequest<TResponse>
{
    protected XmlReader? XmlReader { get; private set; }

    protected string ResourceData { get; set; } = "";
    protected XElement ResourceElements { get; set; }

    protected XmlReaderRequestHandler(ILogger logger) : base(logger)
    {
        //no-op
    }

    
    protected TData LoadXmlAndProcessData(XmlReaderSettings? xmlReaderSettings = null)
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

    protected TData FileLoadXmlAndProcessData()
    {
        try
        {
            ResourceData = File.ReadAllText(ResourcePath);
            ResourceElements = XElement.Parse(ResourceData);
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