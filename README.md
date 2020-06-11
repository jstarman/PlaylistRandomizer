# PlaylistRandomizer

Spotify playlist song shuffler

## Setup

1. Go here and setup an app https://developer.spotify.com/dashboard/login
1. Add a `.\appsettings.json` with:

``` C#
{
    "SpotifyAuthorizeConfig": {
        "ClientID": [your spotify registered application id],
        "ClientSecret": [your spotify secret],
        "HmacSecret": [Any random string]
    }
}
```

1. `dotnet run`

## Shuffle playlist

1. https://localhost:5001/spotify/authorize
1. GET https://localhost:5001/spotify/playlists
1. POST https://localhost:5001/spotify/shuffle

    ``` C#
    {
        "playlistId": [playlist id]
    }
    ```

## TODO

- [x] Authenticate
- [x] Get playlists and display
- [x] Select playlist
- [x] Copy playlist
- [x] Get playlist tracks
- [x] Shuffle playlist tracks
- [x] Add shuffled playlist tracks to copied playlist
- [x] Reload and verify added playlist and tracks
- [ ] Delete original playlist
- [ ] Rename copy to original
- [ ] Create a UI of some sort
