# PlaylistRandomizer

Spotify playlist song shuffler. Copies a playlist appends `_copy` to the name and randomizes the song order in the list.

There is at least one other way to reorder lists without having to copy into another. This method has the fewest API calls.

Spotify does not have a `DELETE` playlist on their public API. Go into the spotify desktop app and right click on playlist to "delete". The playlist is only soft deleted. It is still accessible for other user accounts.

## Setup

1. Download .Net Core SDK <https://dotnet.microsoft.com/download>
1. Setup an app <https://developer.spotify.com/dashboard/login>
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

## Run

1. `dotnet run`
1. <https://localhost:5001>
1. Click Authorize
1. Shuffle a playlist
