using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PlaylistRandomizer.Spotify
{
    public class Api
    {
        private IHttpClientFactory _httpClient;
        private SpotifyAuthorizeConfig _authorizeConfig;
        private SpotifyTokenConfig _tokenConfig;
        private ILogger _logger;
        

        public Api(IHttpClientFactory clientFactory, SpotifyAuthorizeConfig authorizeConfig, SpotifyTokenConfig tokenConfig, ILogger logger)
        {
            _httpClient = clientFactory;
            _authorizeConfig = authorizeConfig;
            _tokenConfig = tokenConfig;
            _logger = logger;
        }

        public AuthorizeRequest Authorize()
        {
            var request = new AuthorizeRequest
            {
                Hmac = Hmac.Generate(DateTime.UtcNow.ToString(), _authorizeConfig.HmacSecret)
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Add(_authorizeConfig.ClientIdKey, _authorizeConfig.ClientID);
            query.Add(_authorizeConfig.ResponseTypeKey, _authorizeConfig.ResponseType);
            query.Add(_authorizeConfig.HmacKey, request.Hmac);
            query.Add(_authorizeConfig.ScopesKey, _authorizeConfig.ScopesValue);
            query.Add(_authorizeConfig.RedirectUriKey, _authorizeConfig.RedirectUri);
            var authUri = new UriBuilder(_authorizeConfig.AuthorizeUri)
            {
                Query = query.ToString()
            };

            request.Uri = authUri.Uri.ToString();

            return request;
        }

        public async Task<TokenResponse> Authenticate(AuthorizationResponse response, string requestHmac)
        {
            if (requestHmac != response.State)
            {
                throw new Exception("Response query string state does not match");
            }

            _logger.Information("Getting token");

            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(_tokenConfig.GrantTypeKey, _tokenConfig.GrantType),
                new KeyValuePair<string, string>(_tokenConfig.CodeKey, response.Code),
                new KeyValuePair<string, string>(_authorizeConfig.RedirectUriKey, _authorizeConfig.RedirectUri)
            });

            var encodeAuthorization = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ _authorizeConfig.ClientID}:{ _authorizeConfig.ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, _tokenConfig.TokenUri);
            request.Headers.Add("Authorization", $"Basic {encodeAuthorization}");
            request.Content = tokenRequestContent;

            var client = _httpClient.CreateClient();
            var tokenResponse = await client.SendAsync(request);
            var sessionToken = await GetContent<TokenResponse>(tokenResponse.Content);
            
            if (string.IsNullOrWhiteSpace(sessionToken.Token))
            {
                throw new Exception("Authentication Error: token was not retrieved.");
            }

            return sessionToken;
        }

        public async Task<Playlist> CreatePlaylist(Playlist playlist, string uri, string token)
        {
            var copy = playlist.Copy();

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", $"Bearer {token}");            
            request.Content = new StringContent(JsonSerializer.Serialize(copy));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var client = _httpClient.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{copy.Name} created");
                return await GetContent<Playlist>(response.Content);
            }
            else
            {
                var error = new Exception($"Create playlist failed with: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                _logger.Error(error, string.Empty);
                throw error;
            }
        }

        public async Task AddTracks(AddTracksRequest requestedTracks, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, SpotifyApi.PlaylistTracks(requestedTracks.PlaylistId));
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestedTracks.Tracks));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var client = _httpClient.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) 
            {
                var error = new Exception($"Add tracks failed with: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                _logger.Error(error, string.Empty);
                throw error;
            }
        }

        public async Task<T> Get<T>(string uri, string token)
        {
            if (string.IsNullOrWhiteSpace(uri)) throw new Exception("No uri supplied");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {token}");
            var client = _httpClient.CreateClient();
            var response = await client.SendAsync(request);
            return await GetContent<T>(response.Content);
        }

        private async Task<T> GetContent<T>(HttpContent content)
        {
            var body = await content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body);
        }
    }    
}
