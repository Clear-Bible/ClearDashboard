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
                throw new MediatorErrorEngineException(result.Message);
            }
            else if (throwIfDataNull && result.Data is null)
            {
                throw new MediatorErrorEngineException("Result Data was null");
            }
        }
    }
}
