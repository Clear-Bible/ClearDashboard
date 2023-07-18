using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitLabProjectUser
    {
        public bool IsOwner { get; set; }

        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("username")] 
        public string UserName { get; set; }

        
        [JsonPropertyName("name")] 
        public string Name { get; set; }


        [JsonPropertyName("state")] 
        public string State { get; set; }

        
        [JsonPropertyName("access_level")] 
        public int AccessLevel { get; set; }

        public PermissionLevel GetPermissionLevel
        {
            get {
                if (IsOwner)
                {
                    return PermissionLevel.Owner;
                }


                switch (AccessLevel)
                {
                    case 30:
                        return PermissionLevel.ReadOnly;
                    case 40:
                        return PermissionLevel.ReadWrite;
                    case 50:
                        return PermissionLevel.Owner;
                }
                return PermissionLevel.ReadWrite;
            }
        }

    }
}
