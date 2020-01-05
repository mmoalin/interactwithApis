using ArtistStats_web.Models;
using System.Collections.Generic;

namespace ArtistStats_web.Services
{
    public interface IMusicInterpretor
    {
        Track[] GetUniqueTracks(List<Release> releases);
        List<Release> GetUniqueTracks(Artist artist);
        Artist PickArtist(string artistName);
        Lyrics GetLyrics(Track track);
        int calculateAverageWords(Artist Artist);
    }
}