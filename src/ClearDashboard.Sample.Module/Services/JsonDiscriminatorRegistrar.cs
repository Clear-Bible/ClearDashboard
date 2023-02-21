using ClearDashboard.Sample.Module.Models;
using ClearDashboard.Wpf.Application.Services;
using Dahomey.Json.Serialization.Conventions;

namespace ClearDashboard.Sample.Module.Services
{
    public class JsonDiscriminatorRegistrar : IJsonDiscriminatorRegistrar
    {
        public void Register(DiscriminatorConventionRegistry registry)
        {
            registry.RegisterType<SampleCorpusAnalysisEnhancedViewItemMetadatum>();
        }
    }
}
