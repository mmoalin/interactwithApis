using System;

namespace ArtistStats_web.Models
{
    public class ArtistStats
    {
        public string ArtistName { get; set; }
        public double AverageWords { get; set; }
        public double Variance { get; set; }
        public double StandardDeviation { get; set; }
        public string LongestTrack { get; set; }
        public string ShortestTrack { get; set; }
    }
}
