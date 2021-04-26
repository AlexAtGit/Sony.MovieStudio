using System.Text.Json.Serialization;

namespace Sony.MovieStudio.Api.Models
{
    public class MovieWatchDuration
    {
        [JsonPropertyName("movieId")]
        public int MoveId { get; set; }

        [JsonPropertyName("watchDurationMs")]
        public int WatchDurationMs { get; set; }
    }
}
