using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ArtistStats_web;
using ArtistStats_web.Models;
using ArtistStats_web.Services;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class TracksController : ControllerBase
{
    private List<Track> _tracks;
    [HttpGet]
    public IEnumerable<Track> Get()
    {
        string[] keyVal = Request.QueryString.Value
            .Substring(Request.QueryString.Value
                .IndexOf('?') + 1)
                    .Split('=');
        MusicInterpretor lo = new MusicInterpretor(new MusicStatService());//TODO: dependency injection
        Artist artist;
        if (keyVal[0] == "artistname")//ArtistResults
        {
            artist = lo.PickArtist(keyVal[1]);

            Track[] tracks = lo.GetUniqueTracks(new List<Release>(artist.Releases));
            _tracks = new List<Track>(tracks);
                //artist.Releases.Where(x => (x != null && x.Media != null)).SelectMany(x => x.Media).SelectMany(x => x.Tracks).ToList();
        }
        return _tracks;
    }

    private string Json(List<Track> tracks)
    {
        return JsonSerializer.Serialize(tracks);
    }
}

