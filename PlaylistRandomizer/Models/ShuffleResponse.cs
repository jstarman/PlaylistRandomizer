using System.Text.Json.Serialization;

namespace PlaylistRandomizer.Models
{
    public class ShuffleResponse
    {
        [JsonPropertyName("playlistName")]
        public string PlaylistName { get; set; }
        [JsonPropertyName("tracksAdded")]
        public int TracksAdded { get; set; }
    }
}
