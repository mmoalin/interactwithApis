using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArtistStats_web.Models
{
    public class Lyrics
    {
        public Track Track { get; set; }
        [JsonPropertyName("lyrics")]
        public string Content { get; set; }
    }
}
