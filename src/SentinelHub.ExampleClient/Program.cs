using SentinelHub;

using IHost host = Host.CreateDefaultBuilder(args)
     .ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
.ConfigureServices((hostContext, services) =>
{
    // Register your services
    services.AddHttpClient(); // For HttpClientFactory
    services.AddSingleton<SentinelHubAuth>();
    services.AddSingleton<SentinelHubClient>();
    services.AddHostedService<ConsoleApp>();
})
.ConfigureAppConfiguration((hostContext, config) =>
{
    // Add configuration sources
    config.AddJsonFile("appsettings.json", optional: true);
    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
    config.AddEnvironmentVariables();
})
.Build();

// Run the host
await host.RunAsync();

public class ConsoleApp : IHostedService
{
    private readonly SentinelHubClient _client;

    public ConsoleApp(SentinelHubClient client)
    {
        _client = client;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bbox = new double[] { 13.822174072265625, 45.85080395917834, 14.55963134765625, 46.29191774991382 };
        var imageData = await _client.GetMapImageAsync("NATURAL-COLOR", bbox, 512, 512, "image/png");

        System.IO.File.WriteAllBytes("map.png", imageData);
        Console.WriteLine("Map image saved as 'map.png'.");

        // Stop the application after completion
        Environment.Exit(0);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Perform any cleanup here if necessary
        return Task.CompletedTask;
    }
}