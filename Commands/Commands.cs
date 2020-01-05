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
        public Task Task { get; private set; }

        public void setCommandTask(Task task)
        {
            Task = task;
        }
        public abstract Task ExecuteAsync();
    }
    public class GetLyrics : Command
    {
        public Lyrics Lyrics { get; set; }
        private readonly string _artistName;
        private readonly string _trackName;
        private readonly Services.IMusicStatService _service;
        private Track _track;
        public GetLyrics(Track track, Services.IMusicStatService service)
        {
            _track = track;
            _artistName = track.Artist.Name;
            _trackName = track.Title;
            _service = service;
        }
        public override Task ExecuteAsync()
        {
            try
            {
                var lyricsJsonTask = _service.getLyrics(_artistName, _trackName);
                setCommandTask(lyricsJsonTask);
                lyricsJsonTask.Wait();
                if (lyricsJsonTask.Result.ToLower() == "notfound")
                {
                    Lyrics = new Lyrics()
                    {
                        Content = ""
                    };
                }
                else
                {
                    Lyrics = new DeserializeJson<Lyrics>().Deserialize(lyricsJsonTask.Result);
                }
                Lyrics.Track = _track;
                return System.Threading.Tasks.Task.CompletedTask;
            }
            catch (System.AggregateException ex)
            {
                return System.Threading.Tasks.Task.FromException(ex);
            }
        }
    }
    public class GetTracksByReleaseID : Command
    {
        private Services.IMusicStatService _service;
        public Release Release { set; get; }
        public GetTracksByReleaseID(Release release, Services.IMusicStatService service)
        {
            Release = release;
            _service = service;
        }
        public override Task ExecuteAsync()
        {
            var releasesJsonTask = _service.getTracks(Release.ID);
            releasesJsonTask.Wait();
            var releasesJson = releasesJsonTask.Result;
            if (releasesJson == "NotFound" || releasesJson == "ServiceUnavailable")
            {
                return Task.FromException(new System.Exception(string.Format("{0}:{1} - {2}", Release.ID, Release.Title, releasesJson)));
            }
            Release temp = new DeserializeJson<Release>().Deserialize(releasesJson);
            List<Track> tracks = temp.Media.Where(x => x != null && x.Tracks != null).SelectMany(x => x.Tracks).ToList();
            for (int j = 0; j < tracks.Count; j++)
            {
                tracks[j].Artist = Release.Artist;
                tracks[j].Release = Release.Title;
                tracks[j].ReleaseID = Release.ID;
            }
            Release.Media = new List<Media>()
            {
                temp.Media[0]
            };
            Release.Media[0].Tracks = tracks;
            Release.Media[0].TrackCount = tracks.Count;
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
    public class GetReleasesByArtistsID : Command
    {
        private Artist _artist;
        public ReleaseResults ReleaseResults { set; get; }
        private Services.IMusicStatService _service;
        public GetReleasesByArtistsID(Artist artist, Services.IMusicStatService service)
        {
            _artist = artist;
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
            for (int i = 0; i < release.Count; i++)
            {
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
            CommandFactory commandFactory = new GetTracksByReleaseIDFactory(release, _service);
            Command command = commandFactory.GetCommand();
            Task t = command.ExecuteAsync();
            t.Wait();
            GetTracksByReleaseID getTracksByReleaseID = (GetTracksByReleaseID)command;
            return getTracksByReleaseID.Release != null ? getTracksByReleaseID.Release : release;
        }
        public Command FillTracksCommands(Release release)
        {
            CommandFactory commandFactory = new GetTracksByReleaseIDFactory(release, _service);
            Command command = commandFactory.GetCommand();
            command.setCommandTask(command.ExecuteAsync());
            return command;
        }
        public override Task ExecuteAsync()
        {
            var releasesJsonTask = _service.getReleases(_artist.ID);
            releasesJsonTask.Wait();
            var releasesJson = releasesJsonTask.Result;
            ReleaseResults = new DeserializeJson<ReleaseResults>().Deserialize(releasesJson);
            List<Command> commands = new List<Command>();
            for (int i = 0; i < ReleaseResults.Releases.Count; i++)
            {
                ReleaseResults.Releases[i].Artist = _artist;
                commands.Add(FillTracksCommands(ReleaseResults.Releases[i]));
            }
            Task[] tasks = commands.Select(x => x.Task).ToArray();
            System.Exception exceptions;
            try
            {
                Task.WaitAll(tasks);
            }
            catch (System.Exception e)
            {
                exceptions = e;
                //throw;
            }
            var Releases = commands.Select(x => (GetTracksByReleaseID)x).Select(r => r.Release).ToList();
            var totalTracks = Releases.Where(x => (x != null && x.Media != null)).SelectMany(x => x.Media).SelectMany(x => x.Tracks).ToList();
            ReleaseResults.Releases = Releases.Where(x => (x != null && x.Media != null)).ToList();
            return System.Threading.Tasks.Task.CompletedTask;
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
            return System.Threading.Tasks.Task.CompletedTask;

        }
    }
}
