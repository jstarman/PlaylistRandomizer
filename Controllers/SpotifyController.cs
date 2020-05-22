using Microsoft.AspNetCore.Mvc;
using PlaylistRandomizer.Spotify;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistRandomizer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly Api _spotify;
        private readonly PlaylistManager _playlistManager;
        private readonly ILogger _logger;

        public SpotifyController(Api spotify, PlaylistManager manager, ILogger logger)
        {
            _spotify = spotify;
            _playlistManager = manager;
            _logger = logger;
        }

        [HttpGet]
        [Route("authorize")]
        public IActionResult Authorize()
        {
            var request = _spotify.Authorize();
            _playlistManager.Hmac = request.Hmac;
            return Redirect(request.Uri);
        }

        [HttpGet]
        [Route("landing")]
        public async Task<IActionResult> Landing()
        {
            var queryStrings = HttpContext.Request.Query;

            if (queryStrings.TryGetValue("error", out var error))
            {
                _logger.Information($"error:{error}");
                return BadRequest(error);
            }

            _playlistManager.Session = await _spotify.Authenticate(new AuthorizationResponse
            {
                Code = queryStrings["code"],
                State = queryStrings["state"]
            }, _playlistManager.Hmac);


            _playlistManager.Me = await _spotify.Get<MeResponse>(SpotifyApi.Me, _playlistManager.Token);

            return Ok(_playlistManager.Me);
        }

        [HttpGet]
        [Route("token")]
        public string Token()
        {
            return string.IsNullOrWhiteSpace(_playlistManager.Token) ? "Not authenticated" : _playlistManager.Token;
        }

        [HttpGet]
        [Route("playlists")]
        public async Task<IEnumerable<Playlist>> PlayLists() 
        {
            var envelope = await _spotify.Get<Envelope<Playlist>>(SpotifyApi.Playlists(_playlistManager.Me), _playlistManager.Token);
            _playlistManager.Playlists.AddRange(envelope.Items);
            return _playlistManager.Playlists;
        }

        [HttpPost]
        [Route("tracks")]
        public async Task<IEnumerable<Track>> Tracks(PlaylistRequest request) 
        {
            var playlist = _playlistManager.Playlists.First(t => t.Id == request.PlaylistId);
            _playlistManager.LastTrackSet = await _spotify.Get<Envelope<TrackShell>>(playlist.Tracks.Resource, _playlistManager.Token);

            while (!string.IsNullOrWhiteSpace(_playlistManager.LastTrackSet.Next?.ToString()))
            {
                _playlistManager.LastTrackSet = await _spotify.Get<Envelope<TrackShell>>(_playlistManager.LastTrackSet.Next, _playlistManager.Token);
            }

            return _playlistManager.Tracks;
        }

        [HttpPost]
        [Route("playlists")]
        public async Task CreatePlaylist(PlaylistRequest request)
        {
            var playlist = _playlistManager.Playlists.First(t => t.Id == request.PlaylistId);
            var savedCopy = await _spotify.CreatePlaylist(playlist, SpotifyApi.Playlists(_playlistManager.Me), _playlistManager.Token);
            _playlistManager.Playlists.Add(savedCopy);
        }
        
        [HttpPost]
        [Route("test")]
        public IActionResult Test(Envelope<TrackShell> playlists)
        {
            _playlistManager.LastTrackSet = playlists;
            if (!string.IsNullOrWhiteSpace(_playlistManager.LastTrackSet.Next.ToString()))
            {
                return Ok("Found it");
            }

            return Ok("no next");
        }
    }
}
