using System;

namespace ArtistStats_web.Commands
{

    public abstract class CommandFactory
    {
        protected Command _command;
        public abstract Command GetCommand();
    }
    public class GetTracksByReleaseIDFactory : CommandFactory
    {
        public GetTracksByReleaseIDFactory(string releaseID, Services.IMusicStatService service)
        {
            _command = new GetTracksByReleaseID(releaseID, service);
        }
        public override Command GetCommand()
        {
            return _command;
        }
    }
    public class GetReleasesByArtistsIDFactory : CommandFactory
    {
        public GetReleasesByArtistsIDFactory(string artistID, Services.IMusicStatService service)
        {
            _command = new GetReleasesByArtistsID(artistID, service);
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
