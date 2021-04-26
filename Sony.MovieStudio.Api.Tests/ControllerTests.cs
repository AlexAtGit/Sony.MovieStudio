using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Sony.MovieStudio.Api.Controllers;
using Sony.MovieStudio.Api.Interfaces;
using Sony.MovieStudio.Api.Models;
using Xunit;

namespace Sony.MovieStudio.Api.Tests
{
    public class ControllerTests : IDisposable
    {
        private TestHostFixture _testHostFixture = new TestHostFixture();

        private HttpClient _httpClient;
        private IServiceProvider _serviceProvider;

        public ControllerTests()
        {
            _httpClient = _testHostFixture.Client;
            _serviceProvider = _testHostFixture.ServiceProvider;
        }
        public void Dispose()
        {
            _testHostFixture?.Dispose();
            _testHostFixture = null;
        }

        [Fact]
        public void SaveMetadataNullTest()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _httpClient.PostAsync("movies", new StringContent(null, Encoding.UTF8, MediaTypeNames.Application.Json)));
        }

        [Theory]
        [InlineData("   ")]
        [InlineData("{\"movieId\": 3, XXX=\"123\"}")]
        [InlineData("{\"abc\": 3, title=\"xyz\"}")] 
        public void SaveMetadataWithBadJsonTest(string jsonMetadata)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _httpClient.PostAsync("movies", new StringContent(jsonMetadata, Encoding.UTF8, MediaTypeNames.Application.Json)));
        }

        [Fact]
        public async Task SaveMetadataOKTest()
        {
            var metaData = new MovieDetails
            {
                MoveId = 3,
                Title = "Elysium",
                Language = "EN",
                Duration = "1:49:00",
                ReleaseYear = 2013
            };

            string jsonMetadata = JsonConvert.SerializeObject(metaData);

            var logger = Mock.Of<ILogger<MoviesController>>();
            var mockRepository = new Mock<IMovieRepository>();

            var expectedMovieId = 99;
            mockRepository.Setup(x => x.Save(jsonMetadata)).ReturnsAsync(expectedMovieId);

            var controller = new MoviesController(logger, mockRepository.Object);

            var result = await controller.SaveMetadata(jsonMetadata);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedMovieId, returnValue);
        }

        [Fact]
        public async Task MovieStatsTest()
        {
            var response = await _httpClient.GetAsync("api/movies/stats");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var movieStats = System.Text.Json.JsonSerializer.Deserialize<List<MovieStat>>(responseContent);
            Assert.True(movieStats.Count > 0);
        }

        // WRITE OTHER REPRESENTATIVE TESTS
    }
}
