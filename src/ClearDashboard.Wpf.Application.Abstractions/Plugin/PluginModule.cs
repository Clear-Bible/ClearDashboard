using Autofac;

namespace ClearDashboard.Wpf.Application.Plugin
{
    public abstract class PluginModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            RegisterJsonDiscriminatorRegistrar(builder);
            RegisterPluginAbstractions(builder);

        }

        protected abstract void RegisterJsonDiscriminatorRegistrar(ContainerBuilder builder);
        protected abstract void RegisterPluginAbstractions(ContainerBuilder builder);
    }
}
