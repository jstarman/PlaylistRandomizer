using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlaylistRandomizer.Models
{
    public class AddTracksRequest
    {
        [JsonPropertyName("playlistId")]
        public string PlaylistId { get; set; }
        [JsonPropertyName("tracks")]
        public IList<string> Tracks { get; set; }
    }
}
