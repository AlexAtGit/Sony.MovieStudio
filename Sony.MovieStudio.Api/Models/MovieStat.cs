using System.Text.Json.Serialization;

namespace Sony.MovieStudio.Api.Models
{
    public class MovieStat
    {
        [JsonPropertyName("movieId")]
        public int MoveId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("averageWatchDurationS")]
        public int AverageWatchDurationS { get; set; }

        [JsonPropertyName("watches")]
        public int Watches { get; set; }

        [JsonPropertyName("releaseYear")]
        public int ReleaseYear { get; set; }
    }
}
