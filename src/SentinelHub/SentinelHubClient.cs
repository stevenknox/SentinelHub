using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SentinelHub;

public class SentinelHubClient
{
    private readonly SentinelHubAuth _auth;
    private readonly string _instanceId;
    private readonly IHttpClientFactory _httpClientFactory;

    public SentinelHubClient(SentinelHubAuth auth, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _auth = auth;
        _httpClientFactory = httpClientFactory;
        _instanceId = configuration["SentinelHub:InstanceId"];

        if (string.IsNullOrEmpty(_instanceId))
        {
            throw new ArgumentException("InstanceId must be provided in configuration.");
        }
    }

    private async Task<HttpClient> CreateHttpClientAsync()
    {
        var client = _httpClientFactory.CreateClient();
        //var token = await _auth.GetAccessTokenAsync();
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<byte[]> GetMapImageAsync(string layer, double[] bbox, int width, int height, string format)
    {
        using (var client = await CreateHttpClientAsync())
        {
            var url = $"https://services.sentinel-hub.com/ogc/wms/{_instanceId}";
            var parameters = new Dictionary<string, string>
            {
                { "SERVICE", "WMS" },
                { "REQUEST", "GetMap" },
                { "LAYERS", layer },
                { "BBOX", string.Join(",", bbox) },
                { "WIDTH", width.ToString() },
                { "HEIGHT", height.ToString() },
                { "FORMAT", format },
                { "CRS", "EPSG:4326" }
            };
            var queryString = string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
            var response = await client.GetAsync($"{url}?{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
