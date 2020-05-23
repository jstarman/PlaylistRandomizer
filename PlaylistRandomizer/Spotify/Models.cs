using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlaylistRandomizer.Spotify
{
    public class AuthorizeRequest
    {
        public string Hmac { get; set; }
        public string Uri { get; set; }
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
        private const int _pageLimit = 100;
        public static int? _testLimit;

        public static string Me => "https://api.spotify.com/v1/me";
        public static string Playlists(MeResponse me) => $"{me.UserUri}/playlists";
        public static string PlaylistTracks(string playlistId) => $"https://api.spotify.com/v1/playlists/{playlistId}/tracks";
        public static int PageLimit 
        { 
            get { return _testLimit ?? _pageLimit; } 
            set { _testLimit = value; } 
        }
    }

    public class Envelope<T>
    {
        [JsonPropertyName("href")]
        public string Resource { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("next")]
        public string Next { get; set; }
        [JsonPropertyName("previous")]
        public string Previous { get; set; }
        [JsonPropertyName("items")]
        public IEnumerable<T> Items { get; set; }
    }

    public class Playlist
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("href")]
        public string Resource { get; set; }
        [JsonPropertyName("tracks")]
        public Tracks Tracks { get; set; }
        [JsonPropertyName("collaborative")]
        public bool Collaborative { get; set; }
        [JsonPropertyName("public")]
        public bool? Public { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class PlaylistRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("collaborative")]
        public bool Collaborative { get; set; }
        [JsonPropertyName("public")]
        public bool? Public { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class Tracks
    {
        [JsonPropertyName("href")]
        public string Resource { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class TrackShell
    {
        [JsonPropertyName("track")]
        public Track Track { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
