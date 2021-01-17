using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VRNotifier.Services.VRPersistence;
using VROrchestrator.Config;
using VROrchestrator.HttpClients;
using VROrchestrator.HttpClients.VRNotifier;
using VROrchestrator.HttpClients.VRPersistence;
using VROrchestrator.HttpClients.VRScraper;
using VROrchestrator.Services;

namespace VROrchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => config.AddEnvironmentVariables("VRORCHESTRATOR_"))
                .ConfigureServices((hostContext, services) =>
                {
                    var vrPersistenceClientSettings = hostContext.Configuration
                        .GetSection(nameof(VRPersistenceClientSettings))
                        .Get<VRPersistenceClientSettings>();
                    services.AddHttpClient<IVRPersistenceClient, VRPersistenceClient>(selfServiceClient =>
                    {
                        selfServiceClient.BaseAddress = new Uri(vrPersistenceClientSettings.Endpoint);
                    });
                    
                    var vrScraperClientSettings = hostContext.Configuration
                        .GetSection(nameof(VRScraperClientSettings))
                        .Get<VRScraperClientSettings>();
                    services.AddHttpClient<IVRScraperClient, VRScraperClient>(selfServiceClient =>
                    {
                        selfServiceClient.BaseAddress = new Uri(vrScraperClientSettings.Endpoint);
                    });
                    
                    var vrNotifierClientSettings = hostContext.Configuration
                        .GetSection(nameof(VRNotifierClientSettings))
                        .Get<VRNotifierClientSettings>();
                    services.AddHttpClient<IVRNotifierClient, VRNotifierClient>(selfServiceClient =>
                    {
                        selfServiceClient.BaseAddress = new Uri(vrNotifierClientSettings.Endpoint);
                    });

                    services.Configure<VROrchestratorServiceSettings>(
                        hostContext.Configuration.GetSection(nameof(VROrchestratorServiceSettings)));
                    services.Configure<TrackedMediaSettings>(
                        hostContext.Configuration.GetSection(nameof(TrackedMediaSettings)));
                    services.AddSingleton<VROrchestratorService>();
                    services.AddHostedService<VROrchestratorService>();
                });
    }
}