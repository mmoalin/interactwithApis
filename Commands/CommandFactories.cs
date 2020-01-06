using System;

namespace ArtistStats_web.Commands
{

    public abstract class CommandFactory
    {
        protected Command _command;
        public abstract Command GetCommand();
    }
    public class GetLyricsFactory : CommandFactory
    {
        public GetLyricsFactory(Models.Track track, Services.IMusicStatService service)
        {
            _command = new GetLyrics(track, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetLyricsAsyncFactory : CommandFactory
    {
        public GetLyricsAsyncFactory(Models.Track track, Services.IMusicStatService service)
        {
            _command = new GetLyricsAsync(track, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetTracksByReleaseIDFactory : CommandFactory
    {
        public GetTracksByReleaseIDFactory(Models.Release release, Services.IMusicStatService service)
        {
            _command = new GetTracksByReleaseID(release, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetTracksByReleaseIDAsyncFactory : CommandFactory
    {
        public GetTracksByReleaseIDAsyncFactory(Models.Release release, Services.IMusicStatService service)
        {
            _command = new GetTracksByReleaseIDAsync(release, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetReleasesByArtistsIDFactory : CommandFactory
    {
        public GetReleasesByArtistsIDFactory(Models.Artist artist, Services.IMusicStatService service)
        {
            _command = new GetReleasesByArtistsID(artist, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetReleasesByArtistsIDAsyncFactory : CommandFactory
    {
        public GetReleasesByArtistsIDAsyncFactory(Models.Artist artist, Services.IMusicStatService service)
        {
            _command = new GetReleasesByArtistsIDAsync(artist, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetArtistsByNameFactory : CommandFactory
    {
        public GetArtistsByNameFactory(string artistName, Services.IMusicStatService service)
        {
            _command = new GetArtistsByName(artistName, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }

}
