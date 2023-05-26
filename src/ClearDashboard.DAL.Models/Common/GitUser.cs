using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

    }
}
