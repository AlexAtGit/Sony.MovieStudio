using Sony.MovieStudio.Api.Interfaces;
using Sony.MovieStudio.Api.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sony.MovieStudio.Api.Repository
{
    public class MovieRepository : IMovieRepository
    {
        private ConcurrentDictionary<int, MovieDetails> _movies;

        public async Task<List<MovieDetails>> GetMovieById(int movieId)
        {
            if (movieId <= 0) throw new ArgumentException($"Invalid ID: {nameof(movieId)}");

            if (_movies == null) await LoadData();

            var moviesById = new List<MovieDetails>();

            var allMoviesById = _movies.Where(x => x.Value.MoveId == movieId).Distinct();
            foreach (var kv in allMoviesById)
            {
                var existingLanguageMovie = moviesById.Find(x => x.Language.Equals(kv.Value.Language));

                if (existingLanguageMovie == null) moviesById.Add(kv.Value);
            }

            return moviesById;
        }

        public async Task<int> Save(string jsonMetadata)
        {
            var movieDetails = JsonSerializer.Deserialize<MovieDetails>(jsonMetadata);

            var itemId = _movies.Count;
            _movies.TryAdd(itemId, movieDetails);

            await SaveData(itemId, movieDetails);

            return itemId;
        }

        /// <summary>
        /// Returns the viewing statistics for all movies.
        /// ● The movies are ordered by most watched, then by release year with newer releases 
        ///   being considered more important.
        /// ● The data returned only needs to contain information from the supplied csv documents
        ///   and does not need to return data provided by the POST metadata endpoint.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetStats()
        {
            if (_movies == null) await LoadData();

            var movieWatchDurations = await LoadStats();

            var moviesStat = new List<MovieStat>();

            foreach (var kv in movieWatchDurations.OrderByDescending(x => x.Value.Count))
            {
                var moviesById = await GetMovieById(kv.Key);
                if (moviesById.Count == 0) continue;

                foreach (var movieDetails in moviesById)
                {
                    var averageWatchDurationS = (int)(kv.Value.Sum() / (1000.0 * kv.Value.Count));

                    var movieStat = new MovieStat
                    {
                        MoveId = movieDetails.MoveId,
                        Title = movieDetails.Title,
                        ReleaseYear = movieDetails.ReleaseYear,

                        Watches = kv.Value.Count,
                        AverageWatchDurationS = averageWatchDurationS
                    };

                    moviesStat.Add(movieStat);
                }
            }

            return JsonSerializer.Serialize<List<MovieStat>>(moviesStat);
        }

        /// <summary>
        /// Returns all metadata for a given movie.
        /// ● Only the latest piece of metadata(highest Id) should be returned where there 
        ///   are multiple metadata records for a given language.
        /// ● Only metadata with all data fields present should be returned, otherwise it 
        ///   should be considered invalid.
        /// ● If no metadata has been POSTed for a specified movie, a 404 should be returned.
        /// ● Results are ordered alphabetically by language.
        /// </summary>
        /// <param name="movieId"></param>
        /// <returns></returns>
        public async Task<string> GetMovie(int movieId)
        {
            if (_movies == null) await LoadData();

            var groupByLanguage = _movies.Where(x => x.Value.MoveId == movieId)
                                         .GroupBy(x => x.Value.Language);

            // iterate through each language group
            var moviesById = new List<MovieDetails>();
            foreach (var group in groupByLanguage)
            {
                var highestKeyId = group.OrderBy(x => x.Key).First();

                var movie = highestKeyId.Value;

                // Only metadata with all data fields present should be returned
                if (string.IsNullOrWhiteSpace(movie.Title) ||
                    string.IsNullOrWhiteSpace(movie.Duration) ||
                    movie.ReleaseYear == 0) continue;

                moviesById.Add(movie);
            }

            return JsonSerializer.Serialize<List<MovieDetails>>(moviesById);
        }

        #region Private Methods
        private async Task LoadData()
        {
            // Note, for this example, data is available in a CSV file
            var dataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dataFile = Path.Combine(dataFolder, "Data\\metadata.csv");
            if (File.Exists(dataFile))
            {
                var csvLines = await File.ReadAllLinesAsync(dataFile);

                _movies = new ConcurrentDictionary<int, MovieDetails>();

                for (var i=1; i < csvLines.Length; i++) // Skip the first line. It lists headers
                {
                    var csvLine = csvLines[i];
                    var csvCells = csvLine.Split(','); 
                    // NOTE THIS IS A SIMPLE WAS TO GET ROW CELLS. 
                    // CELLS THAT HAVE A COMMA IN THEM WILL LEAD TO SKIPPED ENTRY
                    // THERE IS A WAY TO DEAL WITH SUCH CASES, BUT IT IS BEYOND THE SCOPE OF THIS EXCERCISE!

                    // Normally we would use a proper CSV row parser, but for the purpose
                    // of this exercise, we assume that the file is well formed and the content
                    // is valid, so no need for data checks
                    var movieDetails = new MovieDetails
                    {
                        Title       = csvCells[2],
                        Language    = csvCells[3],
                        Duration    = csvCells[4]
                    };


                    if (int.TryParse(csvCells[1].Trim(), out var movieId))
                    {
                        movieDetails.MoveId = movieId;

                        if (int.TryParse(csvCells[5].Trim(), out var releaseYear))
                        {
                            movieDetails.ReleaseYear = releaseYear;

                            _movies.TryAdd(i, movieDetails);
                        }
                    }
                }
            }
        }
        private async Task<ConcurrentDictionary<int, List<long>>> LoadStats()
        {
            var movieWatchDurations = new ConcurrentDictionary<int, List<long>>();

            // Note, for this example, data is available in a CSV file
            var dataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dataFile = Path.Combine(dataFolder, "Data\\stats.csv");
            if (File.Exists(dataFile))
            {
                var csvLines = await File.ReadAllLinesAsync(dataFile);

                for (var i = 1; i < csvLines.Length; i++) // Skip the first line. It lists headers
                {
                    var csvLine = csvLines[i];
                    var csvCells = csvLine.Split(',');

                    // Normally we would use a proper CSV row parser, but for the purpose
                    // of this exercise, we assume that the file is well formed and the content
                    // is valid, so no need for data checks
                    if (!int.TryParse(csvCells[0].Trim(), out var moveId)) continue;
                    if (!long.TryParse(csvCells[1].Trim(), out var watchDurationMs)) continue;

                    var watchDurationsMs = new List<long>();
                    if (!movieWatchDurations.ContainsKey(moveId))
                    {
                        movieWatchDurations.TryAdd(moveId, watchDurationsMs);
                    }
                    else
                    {
                        watchDurationsMs = movieWatchDurations[moveId];
                    }

                    watchDurationsMs.Add(watchDurationMs);
                }
            }

            return movieWatchDurations;
        }
        private async Task SaveData(int itemId, MovieDetails movieDetails)
        {
            if (_movies == null || _movies.Count == 0) return;

            //TODO
        }
        #endregion Private Methods
    }
}
