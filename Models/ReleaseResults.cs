using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArtistStats_web.Models
{
    public class ReleaseResults
    {
        [JsonPropertyName("release-count")] 
        public int Count { get; set; }
        [JsonPropertyName("release-offset")] 
        public int Offset { get; set; }
        public List<Release> Releases { get; set; }
    }
}
