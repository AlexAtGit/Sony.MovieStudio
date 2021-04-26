using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Sony.MovieStudio.Api.Tests
{
    public class TestHostFixture : IDisposable
    {
        private bool _disposed = false;

        public HttpClient Client { get; }
        public IServiceProvider ServiceProvider { get; }
        
        public TestHostFixture()
        {
            var builder = Program.CreateHostBuilder(null)
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.UseEnvironment("Test");
                });
            var host = builder.Start();
            ServiceProvider = host.Services;
            Client = host.GetTestClient();
            Console.WriteLine("TEST Host Started.");
        }
        
        public void Dispose()
        {
            Client?.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed state (managed objects).
            }

            _disposed = true;
        }
    }
}
