using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlaylistRandomizer.Models;
using PlaylistRandomizer.Spotify;
using Serilog;
using System.Collections.Generic;
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

            return new OkObjectResult(_playlistManager.Me);
        }

        [HttpGet]
        [Route("token")]
        public string Token()
        {
            return string.IsNullOrWhiteSpace(_playlistManager.Token) ? "Not authenticated" : _playlistManager.Token;
        }

        [HttpGet]
        [Route("playlists")]
        public async Task<IActionResult> PlayLists()
        {
            var envelope = await _spotify.Get<Envelope<Playlist>>(SpotifyApi.Playlists(_playlistManager.Me), _playlistManager.Token);
            _playlistManager.Reset();
            _playlistManager.Playlists.AddRange(envelope.Items);
            return new OkObjectResult(_playlistManager.Playlists);
        }

        [HttpPost]
        [Route("shuffle/{id}")]
        public async Task<ShuffleResponse> Shuffle(string id)
        {
            return await _playlistManager.ShufflePlaylist(new Models.PlaylistRequest { PlaylistId = id });
        }
    }
}
