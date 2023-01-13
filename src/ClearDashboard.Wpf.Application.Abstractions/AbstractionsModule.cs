using Autofac;

namespace ClearDashboard.Wpf.Application
{
    public class AbstractionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = typeof(AbstractionsModule).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
