using ClearDashboard.Paranext.Module.Models;
using ClearDashboard.Wpf.Application.Services;
using Dahomey.Json.Serialization.Conventions;

namespace ClearDashboard.Paranext.Module.Services
{
    public class JsonDiscriminatorRegistrar : IJsonDiscriminatorRegistrar
    {
        public void Register(DiscriminatorConventionRegistry registry)
        {
            registry.RegisterType<ParanextEnhancedViewItemMetadatum>();
        }
    }
}
