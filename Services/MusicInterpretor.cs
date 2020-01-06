using ArtistStats_web.Commands;
using ArtistStats_web.Helpers;
using ArtistStats_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtistStats_web.Services
{
    public class MusicInterpretor : IMusicInterpretor
    {
        public Lyrics[] Lyrics { get; set; }
        public Artist Artist { get; set; }
        private IMusicStatService _service;
        public MusicInterpretor(IMusicStatService service)
        {
            _service = service;
        }
        public string[] GetLyricsWordSplit(string lyrics)
        {
            lyrics = lyrics.Replace(System.Environment.NewLine, " ");
            string[] words = lyrics.Split(' ');
            return words;
        }
        
        private Lyrics[] getAllLyrics(Artist artist)
        {
            List<Task<string>> lyricsTasks = new List<Task<string>>();
            Track[] tracks = artist.Releases.Where(x => (x != null && x.Media != null))
                                                        .SelectMany(x => x.Media)
                                                        .SelectMany(x => x.Tracks).ToArray();

            List<GetLyricsAsync> commands = new List<GetLyricsAsync>();
            for (int i = 0; i < tracks.Length; i++)
            {
                CommandFactory commandFactory = new GetLyricsAsyncFactory(tracks[i], _service);
                Command command = commandFactory.GetCommand();
                command.setCommandTask(command.ExecuteAsync());
                GetLyricsAsync getLyricsAsync = (GetLyricsAsync)command;
                commands.Add(getLyricsAsync);
                lyricsTasks.Add(getLyricsAsync.Task);
            }
            System.Exception exceptions;
            try
            {
                Task.WaitAll(lyricsTasks.ToArray());
            }
            catch (System.Exception e)
            {
                exceptions = e;
            }
            DeserializeJson<Lyrics> deserializeJson = new DeserializeJson<Lyrics>();
            commands.RemoveAll(x => (x.Task.Result == "{\"lyrics\":\"\"}" || x.Task.Result == "NotFound"));
            for (int i = 0; i < commands.Count; i++)
            {

                commands[i].Lyrics = deserializeJson.Deserialize(commands[i].Task.Result);
                commands[i].Lyrics.Track = commands[i].Track;
            }
            return commands.Select(x => x.Lyrics).ToArray();
        }
        private List<int> getWordsCountFromLyrics(Lyrics[] lyrics)
        {
            List<string[]> wordsPerSong = new List<string[]>();
            lyrics = lyrics.Where(x => x.Content != "").ToArray();
            for (int i = 0; i < lyrics.Length; i++)
            {
                wordsPerSong.Add(GetLyricsWordSplit(lyrics[i].Content));
            }
            return wordsPerSong.Select(x => x.Length).ToList();
        }
        public double calculateAverageWords(Lyrics[] rawlyrics)
        {
            return getWordsCountFromLyrics(rawlyrics).Average();
        }
        public double CalculateStdDev(Lyrics[] rawlyrics)
        {
            var values = getWordsCountFromLyrics(rawlyrics).ToArray();
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
        public double CalculateVariance(Lyrics[] rawlyrics)
        {
            var values = getWordsCountFromLyrics(rawlyrics).ToArray();
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                ret = values.Sum(d => Math.Pow(d - avg, 2));
                return ret / values.Length;
            }
            return -1;
        }
        public Task GetLyrics(Track track)
        {
            CommandFactory commandFactory = new GetLyricsFactory(track, _service);
            Command command = commandFactory.GetCommand();
            return command.ExecuteAsync();
        }
        public Artist PickArtist(string artistName)
        {
            try
            {
                List<Artist> Artists = new List<Artist>();
                CommandFactory commandFactory = new GetArtistsByNameFactory(artistName, _service);
                Command command = commandFactory.GetCommand();
                command.ExecuteAsync();
                GetArtistsByName artistsRepo = (GetArtistsByName)command;
                commandFactory = new GetReleasesByArtistsIDAsyncFactory(artistsRepo.Artists[0], _service);
                command = commandFactory.GetCommand();
                command.ExecuteAsync();
                var releasesRepo = (GetReleasesByArtistsIDAsync)command;
                ReleaseResults releaseResults = releasesRepo.ReleaseResults;
                bool firstArtistHasreleases = releaseResults.Releases.Count > 0;
                if (firstArtistHasreleases)
                {
                    artistsRepo.Artists[0].Releases = releasesRepo.ReleaseResults.Releases.ToArray();
                    Artist = artistsRepo.Artists[0];
                    return Artist;
                }
                else
                {
                    commandFactory = new GetReleasesByArtistsIDAsyncFactory(artistsRepo.Artists[1], _service);
                    command = commandFactory.GetCommand();
                    command.ExecuteAsync();
                    releasesRepo = (GetReleasesByArtistsIDAsync)command;
                    if (releasesRepo.ReleaseResults.Releases.Count > 0)
                    {
                        artistsRepo.Artists[1].Releases = releasesRepo.ReleaseResults.Releases.ToArray();
                        Artist = artistsRepo.Artists[1];
                        return Artist;
                    }
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw ex;
            }
            throw new Exception("No Release data found on top 2 artists");
        }

        public ArtistStats calculateArtistStats(Artist Artist)
        {
            ArtistStats stats = new ArtistStats();
            Lyrics[] lyrics = getAllLyrics(Artist);
            stats.ArtistName = Artist.Name;
            stats.AverageWords = calculateAverageWords(lyrics);
            stats.StandardDeviation = CalculateStdDev(lyrics);
            stats.Variance = CalculateVariance(lyrics);
            List<Track> tracks = lyrics.Select(x => x.Track).ToList();
            /*tracks.Sort((t1, t2) => t1.Length.CompareTo(t2.Length));
            int longestTrackLength = (int)tracks[0].Length / 60000;
            int shortestTrackLength = (int)tracks[tracks.Count - 1].Length / 60000;
            stats.LongestTrack = string.Format("{0} - {1} minutes", tracks[0].Title, longestTrackLength);
            stats.ShortestTrack = string.Format("{0} - {1} minutes", tracks[tracks.Count - 1].Title, shortestTrackLength);*/
            return stats;
        }
    }
}
//list usage in commands of methods belonging to iservices n create it