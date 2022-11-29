using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features.Features
{
    public abstract class CsvReaderRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
        where TRequest : IRequest<TResponse>
    {

        protected CsvReaderRequestHandler(ILogger logger) : base(logger)
        {
            //no-op
        }


        protected TData LoadCsvAndProcessData(string filePath)
        {
            try
            {
                if (File.Exists(filePath) == false)
                {
                    Logger.LogError($"Cannot find file at this path: '{filePath}'");
                }
                return ProcessData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while processing the CSV file: '{ResourcePath}'");
                throw;
            }
        }
    }
}
