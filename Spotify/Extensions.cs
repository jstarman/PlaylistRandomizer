using System.Runtime.CompilerServices;

namespace PlaylistRandomizer.Spotify
{
    public static class Extensions
    {
        public static PlaylistRequest Copy(this Playlist list)
        {
            return new PlaylistRequest
            {
                Name = $"{list.Name}_copy",
                Description = list.Description,
                Public = list.Public,
                Collaborative = list.Collaborative
            };
        }
    }
}
