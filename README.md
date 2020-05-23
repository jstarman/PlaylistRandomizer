# PlaylistRandomizer
Spotify playlist song shuffler

## Setup
1. Go here and setup an app https://developer.spotify.com/dashboard/login
1. Add a `.\appsettings.json` with:
```
{
    "SpotifyAuthorizeConfig": {
        "ClientID": [your spotify registered application id],
        "ClientSecret": [your spotify secret],
        "HmacSecret": [Any random string]
    }
}
```
1. `dotnet run`
1. Run some controller actions

## TODO
x. Authenticate
x. Get playlists and display
x. Select playlist
x. Copy playlist
x. Get playlist tracks
x. Shuffle playlist tracks
x. Add shuffled playlist tracks to copied playlist
x. Reload and verify added playlist and tracks
1. Optional: delete original playlist
1. Optional: rename copy to original
1. Optional: API allows replacing up to 100 tracks instead having to copy to new
1. Turn into command line app