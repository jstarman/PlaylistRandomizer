using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace PlaylistRandomizer
{
    public class Spotify
    {
        private IHttpClientFactory _httpClient;
        private SpotifyAuthorizeConfig _authorizeConfig;
        private SpotifyTokenConfig _tokenConfig;
        private ILogger _logger;
        private string _hmacContent;
        private string _sessionHmac;
        private TokenResponse _sessionToken;
        private MeResponse _me;

        public Spotify(IHttpClientFactory clientFactory, SpotifyAuthorizeConfig authorizeConfig, SpotifyTokenConfig tokenConfig, ILogger logger)
        {
            _httpClient = clientFactory;
            _authorizeConfig = authorizeConfig;
            _tokenConfig = tokenConfig;
            _logger = logger;
        }

        public string Token => _sessionToken.Token;

        public string Authorize()
        {
            _hmacContent = DateTime.UtcNow.ToString();
            _sessionHmac = Hmac.Generate(_hmacContent, _authorizeConfig.HmacSecret);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Add(_authorizeConfig.ClientIdKey, _authorizeConfig.ClientID);
            query.Add(_authorizeConfig.ResponseTypeKey, _authorizeConfig.ResponseType);
            query.Add(_authorizeConfig.HmacKey, _sessionHmac);
            query.Add(_authorizeConfig.ScopesKey, _authorizeConfig.ScopesValue);
            query.Add(_authorizeConfig.RedirectUriKey, _authorizeConfig.RedirectUri);
            var authUri = new UriBuilder(_authorizeConfig.AuthorizeUri)
            {
                Query = query.ToString()
            };
            return authUri.Uri.ToString();
        }

        public async Task<string> Authenticate(AuthorizationResponse response)
        {
            string result;
            if (_sessionHmac != response.State)
            {
                result = "Response query string state does not match";
                _logger.Error(result);
                return result;
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

            _sessionToken = await GetContent<TokenResponse>(tokenResponse.Content);
            
            if (string.IsNullOrWhiteSpace(_sessionToken.Token))
            {
                _me = await Get<MeResponse>(SpotifyApi.Me);
                result = "Authentication Error: token was not retieved.";
                _logger.Error(result);
            }
            else
            {
                result = "Authentication Successful: Token Received";
                _logger.Information(result);
            }

            return result;
        }

        public async Task<T> Get<T>(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {_sessionToken.Token}");
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

    public class SpotifyAuthorizeConfig
    {
        public Uri AuthorizeUri => new Uri("https://accounts.spotify.com/authorize");
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string ClientIdKey => "client_id";
        public string ResponseTypeKey => "response_type";
        public string ResponseType => "code";
        public string RedirectUriKey => "redirect_uri";
        public string RedirectUri => "https://localhost:5001/spotify/landing";
        public string ScopesKey => "scope";
        public string ScopesValue => "user-read-private playlist-modify-private playlist-read-private playlist-read-collaborative";
        public string HmacKey => "state";
        public string HmacSecret { get; set; }
    }

    public class SpotifyTokenConfig
    {
        public Uri TokenUri => new Uri("https://accounts.spotify.com/api/token");
        public string GrantTypeKey => "grant_type";
        public string GrantType => "authorization_code";
        public string CodeKey => "code";
    }

    public class AuthorizationResponse
    {
        public string State { get; set; }
        public string Code { get; set; }
    }

    public class TokenResponse
    { 
        [JsonPropertyName("access_token")]
        public string Token { get; set; }
        [JsonPropertyName("token_type")]
        public string Type { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresSeconds { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class MeResponse
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        [JsonPropertyName("href")]
        public string UserResource { get; set; }
        public Uri UserUri { get { return new Uri(UserResource); } }
    }

    public class SpotifyApi
    {
        public static Uri Me => new Uri("https://api.spotify.com/v1/me");
        public static Uri Playlists(Uri userUri) => new Uri(userUri, "playlists");
    }

    public class Playlists
    {
        [JsonPropertyName("href")]
        public string Resource { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("items")]
        public IEnumerable<Playlist> Items { get; set; }
    }

    public class Playlist
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("href")]
        public string Resource { get; set; }
    }
}
