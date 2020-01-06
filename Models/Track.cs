using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtistStats_web.Models
{
    public class Track
    {
        public decimal Length { get; set; }
        public Artist Artist { get; set; }
        public string ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public string Release { get; set; }
        public string ReleaseID { get; internal set; }
    }
}
