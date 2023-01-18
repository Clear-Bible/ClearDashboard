using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dahomey.Json.Serialization.Conventions;

namespace ClearDashboard.Wpf.Application.Services
{
    public interface IJsonDiscriminatorRegistrar
    {
        void Register(DiscriminatorConventionRegistry registry);
    }
}
