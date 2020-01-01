using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArtistStats_web.Models
{
    public class ArtistResults
    {
        [JsonIgnore]//does this get effected if u remove it?
        public string SearchTerm { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
        public Artist[] Artists { get; set; }
    }
}
