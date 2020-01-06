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
public class ArtistStatsController : ControllerBase
{
    [HttpGet]
    public ArtistStats Get()
    {
        string[] keyVal = Request.QueryString.Value
            .Substring(Request.QueryString.Value
                .IndexOf('?') + 1)
                    .Split('=');
        MusicInterpretor lo = new MusicInterpretor(new MusicStatService());//TODO: dependency injection
        Artist artist;
        string artistName;
        if (keyVal[0] == "artistname")//ArtistResults
            artistName = keyVal[1];
        else
            artistName = keyVal[0];
        try
        {
        artist = lo.PickArtist(artistName);
        return lo.calculateArtistStats(artist);
        }
        catch (System.ArgumentOutOfRangeException)
        {
            var err = new ArtistStats();
            err.ArtistName = string.Format("An error occurred with {0}..", artistName);
            return err;
        }

    }
}

