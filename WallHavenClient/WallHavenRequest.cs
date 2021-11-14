using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WallHaven.WallHavenClient
{
    public class WallHavenRequest : IWallHavenRequest
    {
        private readonly HttpClient _client;

        public WallHavenRequest(HttpClient client, Config config)
        {
            _client = client;
            _client.BaseAddress = new Uri(config.BaseUrl);

            if (config.APIKey != null)
            {
                _client.DefaultRequestHeaders.Add("X-API-Key", config.APIKey);
            }
        }

        public async Task<WallHavenResponse> GetWallpaper(string wallpaperId)
        {
            var response = await _client.GetAsync($"w/{wallpaperId}").ConfigureAwait(false);
            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = response.IsSuccessStatusCode ? JsonSerializer.Deserialize<WallHavenResponse>(responseJson) : null;
            return responseObject;
        }

        public async Task<WallHavenResponse> Search(string searchParams)
        {
            var response = await _client.GetAsync($"search/{searchParams}");
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = response.IsSuccessStatusCode ? JsonSerializer.Deserialize<WallHavenResponse>(responseJson) : null;
            return responseObject;
        }
    }
}