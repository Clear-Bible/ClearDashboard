using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System;
using System.IO;

namespace ClearDashboard.WebApiParatextPlugin
{
    public class FunctionsAssemblyResolver
    {
        /// <summary>
        /// Assembly Type names for which, if the runtime fails to bind to an 
        /// assembly by name, Assembly.Load is attempted.
        /// Per:  https://stackoverflow.com/a/50776946/2705777
        /// </summary>
        private static readonly string[] AssemblyTypeNameFilter = { "System.ComponentModel.Annotations" };

        public static void RedirectAssembly()
        {
            var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a.FullName).ToList();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            if (AssemblyTypeNameFilter.Any(e => args.Name.StartsWith(e)))
            {
                var requestedAssembly = new AssemblyName(args.Name);
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                try
                {
                    assembly = Assembly.Load(requestedAssembly.Name);
                }
                catch (Exception)
                {
                }
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }

            return assembly;
        }

    }
}
