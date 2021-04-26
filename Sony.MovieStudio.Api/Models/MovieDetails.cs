using System.Text.Json.Serialization;

namespace Sony.MovieStudio.Api.Models
{
    public class MovieDetails
    {
        [JsonPropertyName("movieId")]
        public int MoveId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("releaseYear")]
        public int ReleaseYear { get; set; }
    }
}
