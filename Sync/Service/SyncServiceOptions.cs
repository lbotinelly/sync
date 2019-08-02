using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sync.Service
{
    public class SyncServiceOptions
    {
        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        public class Group
        {
            [JsonProperty("folders")]
            public List<string> Folders { get; set; }
            [JsonProperty("cooldown")]
            public TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(5);
            [JsonProperty("ignoreHidden")]
            public bool IgnoreHidden { get; set; } = true;

            [JsonProperty("unidirectional")]
            public bool Unidirectional { get; set; } = false;

            [JsonProperty("mode")]
            public Emode Mode { get; set; } = Emode.Upsert;

            [JsonProperty("ignore")]
            public List<string> Ignore { get; set; } = new List<string>();

            public enum Emode
            {
                Upsert,
                Mirror
            }

        }
    }
}