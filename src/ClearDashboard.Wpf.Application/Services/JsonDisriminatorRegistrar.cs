using ClearDashboard.Wpf.Application.Models.EnhancedView;
using Dahomey.Json.Serialization.Conventions;

namespace ClearDashboard.Wpf.Application.Services
{
    public class JsonDiscriminatorRegistrar : IJsonDiscriminatorRegistrar
    {
        public void Register(DiscriminatorConventionRegistry registry)
        {
            registry.RegisterType<InterlinearEnhancedViewItemMetadatum>();
            registry.RegisterType<AlignmentEnhancedViewItemMetadatum>();
            registry.RegisterType<TokenizedCorpusEnhancedViewItemMetadatum>();
            registry.RegisterType<AlignmentsBatchReviewEnhancedViewItemMetadatum>();
        }
    }
}
