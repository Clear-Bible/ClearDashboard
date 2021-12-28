using ClearDashboard.Core.ViewModels;
using MvvmCross.IoC;
using MvvmCross.ViewModels;

namespace ClearDashboard.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();
            RegisterAppStart<StartupViewModel>();
        }
    }
}
