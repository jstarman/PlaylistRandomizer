using PlaylistRandomizer.Spotify;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PlaylistRandomizer
{
    public class PlaylistManager
    {
        public PlaylistManager()
        {
            Reset();
        }

        public void Reset()
        {
            Tracks = new List<Track>();
            Playlists = new List<Playlist>();
            _lastTrackSet = null;
        }

        private Envelope<TrackShell> _lastTrackSet;

        public string Hmac { get; set; }
        public TokenResponse Session { get; set; }
        public string Token => Session.Token;
        public MeResponse Me { get; set; }
        public List<Playlist> Playlists { get; set; }
        public List<Track> Tracks { get; private set; }
        public Envelope<TrackShell> LastTrackSet 
        {
            get { return _lastTrackSet; }
            set 
            {
                _lastTrackSet = value;
                Tracks.AddRange(_lastTrackSet.Items.Select(t => t.Track)); 
            } 
        }
    }

    public class PlaylistRequest
    {
        [JsonPropertyName("playlistId")]
        public string PlaylistId { get; set; }
    }

    public class AddTracksRequest 
    {
        [JsonPropertyName("playlistId")]
        public string PlaylistId { get; set; }
        [JsonPropertyName("tracks")]
        public string[] Tracks { get; set; }
    }
}
