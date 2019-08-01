using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Sync.Service
{
    public class SyncServiceOptions
    {
        [JsonProperty("pairs")]
        public List<List<string>> Pairs { get; set; }
    }
}