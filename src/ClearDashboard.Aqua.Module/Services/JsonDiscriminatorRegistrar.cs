using ClearDashboard.Aqua.Module.Models;
using ClearDashboard.Wpf.Application.Services;
using Dahomey.Json.Serialization.Conventions;

namespace ClearDashboard.Aqua.Module.Services
{
    public class JsonDiscriminatorRegistrar: IJsonDiscriminatorRegistrar
    {
        public void Register(DiscriminatorConventionRegistry registry)
        {
            registry.RegisterType<AquaCorpusAnalysisEnhancedViewItemMetadatum>();
        }
    }
}
