﻿using Sony.MovieStudio.Api.Models;
using System.Threading.Tasks;

namespace Sony.MovieStudio.Api.Interfaces
{
    public interface IMovieRepository
    {
        Task<MovieDetails> GetMovieById(int movieId);
        Task<int> Save(string jsonMetadata);
        Task<string> GetStats();
        Task<string> GetMovie(int movieId);
    }
}
