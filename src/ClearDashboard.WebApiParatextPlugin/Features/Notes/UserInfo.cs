using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    internal class UserInfo : IUserInfo
    {
        public UserInfo(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
