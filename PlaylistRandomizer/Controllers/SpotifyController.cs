using Microsoft.AspNetCore.Mvc;
using PlaylistRandomizer.Models;
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
        private readonly IWebApi _spotify;
        private readonly PlaylistManager _playlistManager;
        private readonly ILogger _logger;

        public SpotifyController(IWebApi spotify, PlaylistManager manager, ILogger logger)
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
        [Route("shuffle")]
        public async Task<ShuffleResponse> Shuffle(Models.PlaylistRequest request)
        {
            return await _playlistManager.ShufflePlaylist(request);
        }

        [HttpPost]
        [Route("tracks")]
        public async Task<IEnumerable<Track>> Tracks(Models.PlaylistRequest request)
        {
            return await _playlistManager.GetPlaylistTracks(request);
        }

        [HttpPost]
        [Route("playlists")]
        public async Task<IActionResult> CopyPlaylist(Models.PlaylistRequest request)
        {
            var savedCopy = await _playlistManager.CopyPlaylist(request);
            return Ok($"{savedCopy.Id} - {savedCopy.Name}");
        }

        [HttpPost]
        [Route("playlists/{id}/tracks")]
        public async Task AddTracks(AddTracksRequest request)
        {            
            await _spotify.AddTracks(request, _playlistManager.Token);
        }

        [HttpPost]
        [Route("clear")]
        public IActionResult Clear() 
        {
            _playlistManager.Reset();
            return Ok();
        }
        
        [HttpPost]
        [Route("test")]
        public IActionResult Test(AddTracksRequest request)
        {
            var s = SpotifyApi.PlaylistTracks(request.PlaylistId);
            return Ok(s);
        }
    }
}
