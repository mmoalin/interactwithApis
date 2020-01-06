using ArtistStats_web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtistStats_web.Services
{
    public interface IMusicInterpretor
    {
        Artist PickArtist(string artistName);
        Task GetLyrics(Track track);
        ArtistStats calculateArtistStats(Artist Artist);
    }
}