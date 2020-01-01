using System;
using System.Net.Http;
using System.Threading.Tasks;
using ArtistStats_web;
using ArtistStats_web.Services;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class ArtistStatsController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        string[] keyVal = Request.QueryString.Value
            .Substring(Request.QueryString.Value
                .IndexOf('?') + 1)
                    .Split('=');
        MusicStatService lo = new MusicStatService();
        Task<string> res = null;
        if (keyVal[0] == "artistname")//ArtistResults
            res = lo.getArtists(keyVal[1]);
        else if (keyVal[0] == "artistid")//Artist
            res = lo.getReleases(keyVal[1]);
        else if (keyVal[0] == "releaseid")//Release
            res = lo.getTracks(keyVal[1]);
        res.Wait();
        return res.Result;
    }
}

