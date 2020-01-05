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
        private IMusicStatService _service;
        public MusicInterpretor(IMusicStatService service)
        {
            _service = service;
        }
        public int GetLyricsWordCount(string lyrics)
        {
            string[] words = lyrics.Split(' ');
            return words.Length;
        }
        public int calculateAverageWords(Artist artist)
        {
            Lyrics[] lyrics = getAllLyrics(artist);
            return calculateAverageWords(lyrics);
        }
        private Lyrics[] getAllLyrics(Artist artist)
        {
            List<Task<Lyrics>> lyricsTasks = new List<Task<Lyrics>>();
            Lyrics[] lyrics;
            Track[] tracks = artist.Releases.Where(x => (x != null && x.Media != null))
                                                        .SelectMany(x => x.Media)
                                                        .SelectMany(x => x.Tracks).ToArray();
            tracks = GetUniqueTracks(new List<Release>(artist.Releases));
            Func<object, Lyrics> getLyrics = (object track) =>
            {
                return GetLyrics((Track)track); 
            };
            for (int i = 0; i < tracks.Length; i++)
            {
                lyricsTasks.Add(Task<Lyrics>.Factory.StartNew(getLyrics, tracks[i]));
            }
            System.Exception exceptions;
            try
            {
                Task.WaitAll(lyricsTasks.ToArray());
            }
            catch (System.Exception e)
            {
                exceptions = e;
                //throw;
            }
            lyrics = lyricsTasks.Where(x => x.Result != null).Select(x => x.Result).ToArray();
            return lyrics;
        }
        public int calculateAverageWords(Lyrics[] lyrics)
        {
            int sum = 0;
            for (int i = 0; i < lyrics.Length; i++)
            {
                sum = sum + GetLyricsWordCount(lyrics[i].Content);
            }
            return sum / lyrics.Length;
        }
        /// <summary>
        /// Filters a release if they contain duplicates; merging media so it contains unique tracks. 
        /// </summary>
        /// <param name="Release">Release which contains duplicate tracks within their media</param>
        /// <returns></returns>
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
        public Lyrics GetLyrics(Track track)
        {
            CommandFactory commandFactory = new GetLyricsFactory(track, _service);
            Command command = commandFactory.GetCommand();
            command.ExecuteAsync().Wait();
            GetLyrics getLyrics = (GetLyrics)command;
            getLyrics.Lyrics.Track = track;
            return getLyrics.Lyrics;
        }
        public Track[] GetUniqueTracks(List<Release> releases)
        {
            var filtered = FilterDuplicateTracks(releases);
            return filtered.Where(x => (x != null && x.Media != null))
                .SelectMany(x => x.Media)
                .SelectMany(y => y.Tracks)
                .ToArray();
        }
        public List<Release> GetUniqueTracks(Artist artist)
        {
            return FilterDuplicateTracks(new List<Release>(artist.Releases));
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
                commandFactory = new GetReleasesByArtistsIDFactory(artistsRepo.Artists[0], _service);
                command = commandFactory.GetCommand();
                command.ExecuteAsync();
                var releasesRepo = (GetReleasesByArtistsID)command;
                ReleaseResults releaseResults = releasesRepo.ReleaseResults;
                bool firstArtistHasreleases = releaseResults.Releases.Count > 0;
                if (firstArtistHasreleases)
                {
                    artistsRepo.Artists[0].Releases = releasesRepo.ReleaseResults.Releases.ToArray();
                    return Artists[0];
                }
                else
                {
                    commandFactory = new GetReleasesByArtistsIDFactory(artistsRepo.Artists[1], _service);
                    command = commandFactory.GetCommand();
                    command.ExecuteAsync();
                    releasesRepo = (GetReleasesByArtistsID)command;
                    if (releasesRepo.ReleaseResults.Releases.Count > 0)
                    {
                        artistsRepo.Artists[1].Releases = releasesRepo.ReleaseResults.Releases.ToArray();
                        return artistsRepo.Artists[1];
                    }
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw ex;
            }
            throw new Exception("No Release data found on top 2 artists");
        }

    }
}
//list usage in commands of methods belonging to iservices n create it