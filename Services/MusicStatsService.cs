using System.Net.Http;
using System.Threading.Tasks;

namespace ArtistStats_web.Services
{
    public class MusicStatService : IMusicStatService
    {
        public async Task<string> CalculateAverageWords(string artistName)
        {
            var artistID = await getArtists(artistName);
            //select best artist
            //get all tracks
            //calculate average (sum of words of all tracks / total number of tracks)
            return artistID;
        }
        private async Task<string> FetchData(string URI)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            HttpResponseMessage response = await client.GetAsync(URI);
            var responsedata = "";
            if (response.IsSuccessStatusCode)
            {
                responsedata = await response.Content.ReadAsStringAsync();
            }
            else
                return response.StatusCode.ToString();
            return responsedata;
        }
        public async Task<string> getArtists(string name)
        {
            var uri = $"http://musicbrainz.org/ws/2/artist/?query={name}&fmt=json";
            return await FetchData(uri);
        }
        public async Task<string> getReleases(string artistId)
        {
            var uri = $"http://musicbrainz.org/ws/2/release?artist={artistId}&type=album|ep&fmt=json";
            return await FetchData(uri);
        }
        public async Task<string> getTracks(string releaseId)
        {
            var uri = $"https://musicbrainz.org/ws/2/release/{releaseId}/?inc=recordings+url-rels&fmt=json";
            return await FetchData(uri);
        }
        public async Task<string> getLyrics(string artist, string trackTitle)
        {
            var uri = $"https://api.lyrics.ovh/v1/{artist}/{trackTitle}";
            return await FetchData(uri);
        }

    }

}
