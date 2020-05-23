using System.Text.Json.Serialization;

namespace PlaylistRandomizer.Models
{
    public class PlaylistRequest
    {
        [JsonPropertyName("playlistId")]
        public string PlaylistId { get; set; }
    }
}
