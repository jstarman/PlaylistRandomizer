using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;

namespace PlaylistRandomizer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly Spotify _spotify;
        private readonly ILogger _logger;

        public SpotifyController(Spotify spotify, ILogger logger)
        {
            _spotify = spotify;
            _logger = logger;
        }

        [HttpGet]
        [Route("authorize")]
        public IActionResult Authorize()
        {
            return Redirect(_spotify.Authorize());
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

            var result = await _spotify.Authenticate(new AuthorizationResponse
            {
                Code = queryStrings["code"],
                State = queryStrings["state"]
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("me")]
        public async Task<MeResponse> Me()
        {
            return await _spotify.Get<MeResponse>(SpotifyApi.Me);
        }

        [HttpGet]
        [Route("playlists")]
        public async Task<Envelope<Playlist>> PlayLists() 
        {
            return await _spotify.GetMy<Envelope<Playlist>>(SpotifyApi.Playlists);
        }

        [HttpGet]
        [Route("token")]
        public string Token()
        {
            return _spotify.Token;
        }

        [HttpPost]
        [Route("test")]
        public Envelope<Playlist> Test(Envelope<Playlist> playlists)
        {
            return playlists;
        }
    }
}
