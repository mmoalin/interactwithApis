using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtistStats_web.Models
{
    public class Artist
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public Release[] Releases { get; set; }
    }
}
