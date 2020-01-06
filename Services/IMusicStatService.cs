using System.Threading.Tasks;

namespace ArtistStats_web.Services
{
    public interface IMusicStatService
    {
        Task<string> getArtists(string name);
        Task<string> getLyrics(string artist, string trackTitle);
        Task<string> getReleases(string artistId);
        Task<string> getTracks(string releaseId);
    }
}