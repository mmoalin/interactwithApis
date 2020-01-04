using ArtistStats_web.Helpers;
using ArtistStats_web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArtistStats_web.Commands
{
    public abstract class Command
    {
        public abstract Task ExecuteAsync();
    }
    public class GetTracksByReleaseID : Command
    {
        private string _releaseID;
        private Services.IMusicStatService _service;
        public Release Release { set; get; }
        public GetTracksByReleaseID(string releaseID, Services.IMusicStatService service)
        {
            _releaseID = releaseID;
            _service = service;
        }
        public override Task ExecuteAsync()
        {
            var releasesJsonTask = _service.getTracks(_releaseID);
            releasesJsonTask.Wait();
            var releasesJson = releasesJsonTask.Result;
            if (releasesJson == "NotFound" || releasesJson == "ServiceUnavailable")
            {
                return Task.CompletedTask;
            }
            Release = new DeserializeJson<Release>().Deserialize(releasesJson);
            List<Track> tracks = Release.Media.Where(x => x != null && x.Tracks != null).SelectMany(x => x.Tracks).ToList();
            Release.Media[0].Tracks = tracks;
            Release.Media[0].TrackCount = tracks.Count;
            List<Media> media = new List<Media>()
            {
                Release.Media[0]
            };
            Release.Media = media;
            return Task.CompletedTask;
        }
    }
    public class GetReleasesByArtistsID : Command
    {
        private string _artistID;
        public ReleaseResults ReleaseResults { set; get; }
        private Services.IMusicStatService _service;
        public GetReleasesByArtistsID(string artistID, Services.IMusicStatService service)
        {
            _artistID = artistID;
            _service = service;
        }
        public List<Release> FilterDuplicateTracks(List<Release> release)
        {
            var nullReleases = release.Where(x => x.Media == null).ToList();
            int deleted = release.RemoveAll(x => x.Media == null);
            for (int i = 0; i < release.Count; i++)
            {
                var tracks = release[i].Media.Where(x => x != null && x.Tracks != null).SelectMany(x => x.Tracks).ToList();
                for (int j = 0; j < tracks.Count; j++)
                {
                    tracks[j].Release = release[i].Title;
                    tracks[j].ReleaseID = release[i].ID;
                }
            }
            var allTracks = release.Where(x => x != null && x.Media != null).SelectMany(x => x.Media).SelectMany(y => y.Tracks).ToList();
            var duplicateTracks = allTracks.GroupBy(x => x.Title)
                                        .Where(g => g.Count() > 1)
                                        .Select(y => y)
                                        .ToList();

            var dt = duplicateTracks.SelectMany(x => x).ToList();
            for (int i = 0; i < release.Count; i++) {
                List<string> names = dt.Select(x => x.Title).ToList();
                release[i].Media[0].Tracks.RemoveAll(x => names.Contains(x.Title));
            }
            /*for (int i = 0; i < dt.Count; i++)
            {
                tracks.Remove(dt[i]);
            }*/
            for (int k = 0; k < duplicateTracks.Count; k++)
            {
                var randomCopy = duplicateTracks[k].First();
                release.Where(x => x.ID == randomCopy.ReleaseID).Select(x => x).First().Media[0].Tracks.Add(randomCopy);
            }
            
            return release;
        }
        public Release FillTracks(Release release)
        {
            CommandFactory commandFactory = new GetTracksByReleaseIDFactory(release.ID, _service);
            Command command = commandFactory.GetCommand();
            Task t = command.ExecuteAsync();
            t.Wait();
            GetTracksByReleaseID getTracksByReleaseID = (GetTracksByReleaseID)command;
            return getTracksByReleaseID.Release != null ? getTracksByReleaseID.Release : release;
        }
        public override Task ExecuteAsync()
        {
            var releasesJsonTask = _service.getReleases(_artistID);
            releasesJsonTask.Wait();
            var releasesJson = releasesJsonTask.Result;
            ReleaseResults = new DeserializeJson<ReleaseResults>().Deserialize(releasesJson);
            for (int i = 0; i < ReleaseResults.Releases.Count; i++)
            {
                ReleaseResults.Releases[i] = FillTracks(ReleaseResults.Releases[i]);
            }
            return Task.CompletedTask;
        }
    }
    public class GetArtistsByName : Command
    {
        public string _artistName;
        public List<Artist> Artists { set; get; }
        private Services.IMusicStatService _service;
        public GetArtistsByName(string artistName, Services.IMusicStatService service)
        {
            _artistName = artistName;
            _service = service;
        }
        /*        public async Task<Artist[]> Execute()
                {
                    string artistsJson = await _service.getArtists(_artistName);
                    ArtistResults artistsResults = new DeserializeJson<ArtistResults>().Deserialize(artistsJson);
                    return artistsResults.Artists;
                }
        */
        public override Task ExecuteAsync()
        {
            var artistsJsonTask = _service.getArtists(_artistName);
            artistsJsonTask.Wait();
            var artistsJson = artistsJsonTask.Result;
            ArtistResults artistsResults = new DeserializeJson<ArtistResults>().Deserialize(artistsJson);
            Artists = artistsResults.Artists.ToList();
            return Task.CompletedTask;

        }
    }
}
