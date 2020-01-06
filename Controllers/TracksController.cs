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
        string artistName;
        string option = "";
        if (keyVal[0] == "artistname")//ArtistResults
        {
            if (keyVal[1].Contains('&'))
            {
                var request = keyVal[1].Split('&');
                option = request[1];
                artistName = request[0];
            }
            else
                artistName = keyVal[1];

            artist = lo.PickArtist(artistName);
            Track[] tracks = artist.Releases.Where(x => (x != null && x.Media != null)).SelectMany(x => x.Media).SelectMany(x => x.Tracks).ToArray(); ;

            _tracks = new List<Track>(tracks);
        }
        return _tracks;
    }

    private string Json(List<Track> tracks)
    {
        return JsonSerializer.Serialize(tracks);
    }
}

