using CefSharp.ModelBinding;
using System;

namespace ClearDashboard.Paranext.Module.Services
{
    /// <summary>
    /// Used to customize how values are deserialized into complex types.
    /// </summary>
    public class DashboardJavascriptApiBinder : DefaultBinder
    {
        public override object Bind(object obj, Type targetParamType)
        {
            return base.Bind(obj, targetParamType);
        }
    }
}
