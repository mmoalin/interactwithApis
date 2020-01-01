using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArtistStats_web.Models
{
    public class Release
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string ReleaseType { get; set; }
        [JsonPropertyName("date")]
        public string ReleaseDate { get; set; }
        public Artist Artist { get; set; }
        public List<Media> Media { get; set; }
        public string Packaging { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
    }
    public class Media
    {
        [JsonPropertyName("format-id")]
        public string ID { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }
        [JsonPropertyName("track-count")]
        public int TrackCount { get; set; }
        public List<Track> Tracks { get; set; }

    }
}
