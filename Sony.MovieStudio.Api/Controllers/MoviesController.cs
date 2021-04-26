using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Sony.MovieStudio.Api.Interfaces;

namespace Sony.MovieStudio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        #region Fields
        private readonly ILogger<MoviesController> _logger;

        // We would normally work with a data service, but for this simple API
        // we can just work directly with the movie repository
        private readonly IMovieRepository _repository;
        #endregion Fields

        #region Construction
        public MoviesController(ILogger<MoviesController> logger, IMovieRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
        #endregion Construction

        /// <summary>
        /// Saves a new piece of metadata. The body contains the following information
        /// 1. Movie Id - integer representing unique movie identifier
        /// 2. Title - string representing the title of the Movie
        /// 3. Language - string representing the translation of the metadata
        /// 4. Duration - string representing the length of the movie
        /// 5. Release Year - integer representing the year the movie was released
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        [HttpPost("save/{jsonMetadata}")]
        public async Task<IActionResult> SaveMetadata(string jsonMetadata)
        {
            try
            {
                // Note Data validation is handled within the model definition

                var movieId = await _repository.Save(jsonMetadata);
                return Ok(movieId);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("stats")]
        public async Task<ActionResult> GetMoviesStats()
        {
            try
            {
                // Data validation is handled within the model definition

                var stats = await _repository.GetStats();
                return Ok(stats);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("getMovie/{movieId}")]
        public async Task<ActionResult<string>> GetMovie(int movieId)
        {
            try
            {
                return Ok(await _repository.GetMovieById(movieId));
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
