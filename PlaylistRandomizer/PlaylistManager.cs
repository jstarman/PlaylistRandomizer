using PlaylistRandomizer.Models;
using PlaylistRandomizer.Spotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistRandomizer
{
    public class PlaylistManager
    {
        private Envelope<TrackShell> _lastTrackSet;
        private IWebApi _spotifyWebApi;

        public PlaylistManager(IWebApi spotifyWebApi)
        {
            _spotifyWebApi = spotifyWebApi;
            Reset();
        }

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

        public async Task<IEnumerable<Track>> GetPlaylistTracks(Models.PlaylistRequest request)
        {
            var playlist = Playlists.First(t => t.Id == request.PlaylistId);
            LastTrackSet = await _spotifyWebApi.Get<Envelope<TrackShell>>(playlist.Tracks.Resource, Token);

            while (!string.IsNullOrWhiteSpace(LastTrackSet.Next?.ToString()))
            {
                LastTrackSet = await _spotifyWebApi.Get<Envelope<TrackShell>>(LastTrackSet.Next, Token);
            }

            return Tracks;
        }

        public async Task<Playlist> CopyPlaylist(Models.PlaylistRequest request)
        {
            var playlist = Playlists.First(t => t.Id == request.PlaylistId);
            var savedCopy = await _spotifyWebApi.CreatePlaylist(playlist, SpotifyApi.Playlists(Me), Token);
            Playlists.Add(savedCopy);
            return savedCopy;
        }

        public async Task AddTracks(AddTracksRequest request)
        {
            var page = 0;
            var tracks = request.Tracks.Take(SpotifyApi.PageLimit);

            while (tracks.Any())
            {
                var pagedTrackRequest = new AddTracksRequest { PlaylistId = request.PlaylistId, Tracks = tracks.ToArray() };
                await _spotifyWebApi.AddTracks(pagedTrackRequest, Token);

                page++;
                tracks = request.Tracks.Skip(SpotifyApi.PageLimit * page).Take(SpotifyApi.PageLimit);
            }
        }

        public async Task<ShuffleResponse> ShufflePlaylist(Models.PlaylistRequest request)
        {
            var copiedPlaylist = await CopyPlaylist(request);
            var tracks = await GetPlaylistTracks(request);
            await AddTracks(new AddTracksRequest { PlaylistId = copiedPlaylist.Id, Tracks = tracks.Select(t => t.Uri).Shuffle() });

            Reset();
            var envelope = await _spotifyWebApi.Get<Envelope<Playlist>>(SpotifyApi.Playlists(Me), Token);
            Playlists.AddRange(envelope.Items);
            tracks = await GetPlaylistTracks(new Models.PlaylistRequest { PlaylistId = copiedPlaylist.Id });
            return new ShuffleResponse { PlaylistName = copiedPlaylist.Name, TracksAdded = tracks.Count() };
        }

        public void Reset()
        {
            Tracks = new List<Track>();
            Playlists = new List<Playlist>();
            _lastTrackSet = null;
        }
    }
}