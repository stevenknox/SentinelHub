using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SentinelHub;

public class SentinelHubAuth
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly IHttpClientFactory _httpClientFactory;
    private string _accessToken;
    private DateTime _tokenExpiration;

    public SentinelHubAuth(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _clientId = configuration["SentinelHub:ClientId"];
        _clientSecret = configuration["SentinelHub:ClientSecret"];
        _httpClientFactory = httpClientFactory;

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
        {
            throw new ArgumentException("ClientId and ClientSecret must be provided in configuration.");
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiration)
            return _accessToken;

        var client = _httpClientFactory.CreateClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var authString = $"{_clientId}:{_clientSecret}";
        var base64AuthString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authString));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthString);

        var response = await client.PostAsync("https://services.sentinel-hub.com/oauth/token", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();

        dynamic json = JsonConvert.DeserializeObject(responseContent);
        _accessToken = json.access_token;
        int expiresIn = json.expires_in;
        _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        return _accessToken;
    }
}
