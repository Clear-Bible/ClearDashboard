using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class ResultExtensions
    {
        public static void ThrowIfCanceledOrFailed<T>(this Result<T> result, bool throwIfDataNull = false)
        {
            if (result.Canceled)
            {
                throw new OperationCanceledException();
            }
            else if (!result.Success)
            {
                if (result.Message is not null && result.Message.Contains("The operation was canceled"))
                {
                    throw new OperationCanceledException();
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message ?? string.Empty);
                }
            }
            else if (throwIfDataNull && result.Data is null)
            {
                throw new MediatorErrorEngineException("Result Data was null");
            }
        }
    }
}
