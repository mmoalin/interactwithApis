using ArtistStats_web.Commands;
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
            commandFactory = new GetReleasesByArtistsIDFactory(artistsRepo.Artists[0].ID, _service);
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
                commandFactory = new GetReleasesByArtistsIDFactory(artistsRepo.Artists[1].ID, _service);
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