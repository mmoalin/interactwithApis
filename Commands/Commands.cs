using ArtistStats_web.Helpers;
using ArtistStats_web.Models;
using ArtistStats_web.Services;
using System;
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

    public class GetLyricsAsync : GetLyrics
    {
        public new Task<string> Task;
        public GetLyricsAsync(Track track, IMusicStatService service) : base(track, service)
        {
        }
        public override Task ExecuteAsync()
        {
            var lyricsJsonTask = _service.getLyrics(_artistName, _trackName);
            setCommandTask(lyricsJsonTask);
            Task = lyricsJsonTask;
            return lyricsJsonTask;
        }
    }
    public class GetLyrics : Command
    {
        public Lyrics Lyrics { get; set; }
        protected readonly string _artistName;
        protected readonly string _trackName;
        protected readonly Services.IMusicStatService _service;
        public Track Track { get; set; }
        public GetLyrics(Track track, Services.IMusicStatService service)
        {
            Track = track;
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
                if (lyricsJsonTask.Result == null || lyricsJsonTask.Result.ToLower() == "notfound")
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
                Lyrics.Track = Track;
                return System.Threading.Tasks.Task.CompletedTask;
            }
            catch (System.AggregateException ex)
            {
                return System.Threading.Tasks.Task.FromException(ex);
            }
        }
    }
    public class GetTracksByReleaseIDAsync : Command
    {
        public new Task<string> Task;
        private Services.IMusicStatService _service;
        public Release Release { set; get; }
        public GetTracksByReleaseIDAsync(Release release, Services.IMusicStatService service)
        {
            Release = release;
            _service = service;
        }
        public override Task ExecuteAsync()
        {
            Task = _service.getTracks(Release.ID);
            setCommandTask(Task);
            return Task;
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
            var allTracks = release.Where(x => x != null && x.Media != null).SelectMany(x => x.Media).SelectMany(y => y.Tracks).ToList();
            var duplicateTrackGroups = allTracks.GroupBy(x => x.Title)
                                        .Where(g => g.Count() > 1)
                                        .Select(y => y)
                                        .ToList();

            var duplicateTracks = duplicateTrackGroups.SelectMany(x => x).ToList();
            if (duplicateTracks.Count > 0)
            {
                List<string> names = duplicateTracks.Select(x => x.Title).ToList();

                for (int i = 0; i < release.Count; i++)
                {
                    release[i].Media[0].Tracks.RemoveAll(x => names.Contains(x.Title));
                }
                for (int k = 0; k < duplicateTrackGroups.Count; k++)
                {
                    var randomCopy = duplicateTrackGroups[k].First();
                    release.Where(x => x.ID == randomCopy.ReleaseID).Select(x => x).First().Media[0].Tracks.Add(randomCopy);
                }
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
        public Command FillTracksAsyncCommands(Release release)
        {
            CommandFactory commandFactory = new GetTracksByReleaseIDAsyncFactory(release, _service);
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
            ReleaseResults.Releases = FilterDuplicateTracks(Releases.Where(x => (x != null && x.Media != null)).ToList());
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
    public class GetReleasesByArtistsIDAsync : Command
    {
        public new Task<string> Task;
        private Artist _artist;
        public ReleaseResults ReleaseResults { set; get; }
        private Services.IMusicStatService _service;
        public GetReleasesByArtistsIDAsync(Artist artist, Services.IMusicStatService service)
        {
            _artist = artist;
            _service = service;
        }
        public List<Release> FilterDuplicateTracks(List<Release> release)
        {
            var allTracks = release.Where(x => x != null && x.Media != null).SelectMany(x => x.Media).SelectMany(y => y.Tracks).ToList();
            var duplicateTrackGroups = allTracks.GroupBy(x => x.Title)
                                        .Where(g => g.Count() > 1)
                                        .Select(y => y)
                                        .ToList();

            var duplicateTracks = duplicateTrackGroups.SelectMany(x => x).ToList();
            if (duplicateTracks.Count > 0)
            {
                List<string> names = duplicateTracks.Select(x => x.Title).ToList();

                for (int i = 0; i < release.Count; i++)
                {
                    release[i].Media[0].Tracks.RemoveAll(x => names.Contains(x.Title));
                }
                for (int k = 0; k < duplicateTrackGroups.Count; k++)
                {
                    var randomCopy = duplicateTrackGroups[k].First();
                    release.Where(x => x.ID == randomCopy.ReleaseID).Select(x => x).First().Media[0].Tracks.Add(randomCopy);
                }
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
        public static GetTracksByReleaseIDAsync FillTracksAsyncCommands(Release release, IMusicStatService service)
        {
            CommandFactory commandFactory = new GetTracksByReleaseIDAsyncFactory(release, service);
            Command command = commandFactory.GetCommand();
            command.setCommandTask(command.ExecuteAsync());
            return (GetTracksByReleaseIDAsync)command;
        }
        public override Task ExecuteAsync()
        {
            var releasesJsonTask = _service.getReleases(_artist.ID);
            releasesJsonTask.Wait();
            var releasesJson = releasesJsonTask.Result;
            ReleaseResults = new DeserializeJson<ReleaseResults>().Deserialize(releasesJson);
            List<GetTracksByReleaseIDAsync> commands = new List<GetTracksByReleaseIDAsync>();
            for (int i = 0; i < ReleaseResults.Releases.Count; i++)
            {
                ReleaseResults.Releases[i].Artist = _artist;
                commands.Add(FillTracksAsyncCommands(ReleaseResults.Releases[i], _service));
            }
            Task[] tasks = commands.Select(x => x.Task).ToArray();
            Exception exceptions;
            try
            {
                System.Threading.Tasks.Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                exceptions = e;
            }
            List<Exception> errors = new List<Exception>();
            for (int i = 0; i < commands.Count; i++)
            {
                string releasesWithTracksJson = "";
                try
                {
                    releasesWithTracksJson = commands[i].Task.Result;
                }
                catch(Exception e)
                {
                    exceptions = e;
                }

                if (releasesWithTracksJson == "" || releasesWithTracksJson == "NotFound" || releasesWithTracksJson == "ServiceUnavailable")
                {
                    errors.Add(new System.Exception(string.Format("{0}:{1} - {2}", commands[i].Release.ID, commands[i].Release.Title, releasesWithTracksJson)));
                    continue;
                }
                Release temp = new DeserializeJson<Release>().Deserialize(releasesWithTracksJson);
                if (temp.Media != null)
                {
                    List<Track> tracks = temp.Media.Where(x => x != null && x.Tracks != null).SelectMany(x => x.Tracks).ToList();
                    for (int j = 0; j < tracks.Count; j++)
                    {
                        tracks[j].Artist = commands[i].Release.Artist;
                        tracks[j].Release = commands[i].Release.Title;
                        tracks[j].ReleaseID = commands[i].Release.ID;
                    }
                    commands[i].Release.Media = new List<Media>()
                    {
                        temp.Media[0]
                    };
                    commands[i].Release.Media[0].Tracks = tracks;
                    commands[i].Release.Media[0].TrackCount = tracks.Count;
                }

            }
            var Releases = commands.Select(x => x).Select(r => r.Release).ToList();
            ReleaseResults.Releases = FilterDuplicateTracks(Releases.Where(x => (x != null && x.Media != null)).ToList());
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
