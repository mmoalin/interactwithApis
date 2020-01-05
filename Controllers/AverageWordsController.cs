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
public class AverageWordsController : ControllerBase
{
    private Artist _tracks;
    [HttpGet]
    public int Get()
    {
        string[] keyVal = Request.QueryString.Value
            .Substring(Request.QueryString.Value
                .IndexOf('?') + 1)
                    .Split('=');
        MusicInterpretor lo = new MusicInterpretor(new MusicStatService());//TODO: dependency injection
        string artistName;
        int average = 0;
        if (keyVal[0] == "artistname")//ArtistResults
        {
            artistName = keyVal[1];

            Artist artist = lo.PickArtist(artistName);
            average = lo.calculateAverageWords(artist);
        }
        return average;
    }

    private string Json(List<Track> tracks)
    {
        return JsonSerializer.Serialize(tracks);
    }
}

