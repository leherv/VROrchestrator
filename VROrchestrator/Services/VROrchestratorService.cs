using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VRNotifier.Services.VRPersistence;
using VROrchestrator.Config;
using VROrchestrator.DTO;
using VROrchestrator.DTO.VRNotifier;
using VROrchestrator.DTO.VRPersistence;
using VROrchestrator.HttpClients.VRNotifier;
using VROrchestrator.HttpClients.VRScraper;

namespace VROrchestrator.Services
{
    public class VROrchestratorService : BackgroundService
    {
        private readonly IVRPersistenceClient _vrPersistenceClient;
        private readonly IVRNotifierClient _vrNotifierClient;
        private readonly IVRScraperClient _vrScraperClient;
        private readonly VROrchestratorServiceSettings _orchestratorServiceSettings;
        private readonly TrackedMediaSettings _trackedMediaSettings;
        private readonly ILogger<VROrchestratorService> _logger;

        private ScrapeInstructionsDTO _scrapeInstructionsDto;
        private ScrapeInstructionsDTO ScrapeInstructionsDto => _scrapeInstructionsDto ??= BuildScrapeInstructionsDTO();


        public VROrchestratorService(IVRPersistenceClient vrPersistenceClient,
            IVRNotifierClient vrNotifierClient,
            IVRScraperClient vrScraperClient,
            IOptions<VROrchestratorServiceSettings> orchestratorServiceSettings,
            IOptions<TrackedMediaSettings> trackedMediaSettings,
            ILogger<VROrchestratorService> logger)
        {
            _vrPersistenceClient = vrPersistenceClient;
            _vrNotifierClient = vrNotifierClient;
            _vrScraperClient = vrScraperClient;
            _trackedMediaSettings = trackedMediaSettings.Value;
            _orchestratorServiceSettings = orchestratorServiceSettings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await ExecuteAsync();
                await Task.Delay(TimeSpan.FromMinutes(_orchestratorServiceSettings.ScrapeIntervalMinutes),
                    stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }

        private async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting scraping...");
            var scrapeResult = await _vrScraperClient.Scrape(ScrapeInstructionsDto);
            if (scrapeResult.IsFailure)
            {
                _logger.LogError("Scraping failed due to: {message}", scrapeResult.Error);
                return;
            }
            _logger.LogInformation("Scraping successful.");
            var scrapeResults = scrapeResult.Value;
            var successfulScrapeResults = scrapeResults
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .ToList();
            
            _logger.LogInformation("Starting persisting of scrape results");
            var persistResult = await _vrPersistenceClient.AddReleases(BuildAddReleasesDTO(successfulScrapeResults));
            if (persistResult.IsFailure)
            {
                _logger.LogError("Persisting failed due to: {message}", persistResult.Error);
                return;
            }

            var persistResults = persistResult.Value;
            // select only those scrapeResults that where successfully persisted and use the scrape info
            var successFullyPersistedScrapeResults = successfulScrapeResults.Zip(persistResults)
                .Where(tuple => tuple.Second.IsSuccess)
                .Select(tuple => tuple.First);
            
            _logger.LogInformation("Finished persisting scrape results.");
            
            foreach (var successFullyPersistedScrapeResult in successFullyPersistedScrapeResults)
            {
                var subscribedEndpointsResult = await _vrPersistenceClient.GetSubscribedEndpoints(successFullyPersistedScrapeResult?.MediaName.ToLower());
                if (subscribedEndpointsResult.IsFailure)
                {
                    _logger.LogError("Fetching subscribed endpoints for media {mediaName} failed due to {message}", successFullyPersistedScrapeResult?.MediaName, subscribedEndpointsResult.Error);
                    continue;
                }

                if (!subscribedEndpointsResult.Value.IsSuccess)
                {
                    _logger.LogError("Fetching subscribed endpoints for media {mediaName} failed due to {message}", successFullyPersistedScrapeResult?.MediaName, subscribedEndpointsResult.Value.Error);
                    continue;
                }
                    
                _logger.LogInformation("Notifying endpoints for new release of media {mediaName}", successFullyPersistedScrapeResult?.MediaName);
                var notifyResult = await _vrNotifierClient.Notify(BuildNotificationDTO(
                    successFullyPersistedScrapeResult.ReleaseNumber,
                    successFullyPersistedScrapeResult.SubReleaseNumber,
                    successFullyPersistedScrapeResult.Url,
                    subscribedEndpointsResult.Value.Value.Select(n => n.Identifier).ToList(),
                    successFullyPersistedScrapeResult.MediaName.ToLower()));

                if (notifyResult.IsFailure)
                    _logger.LogError("Notifying endpoints for new release of media {mediaName} failed due to {message}", successFullyPersistedScrapeResult?.MediaName, subscribedEndpointsResult.Error);

                if (!notifyResult.Value.IsSuccess)
                    _logger.LogError("Notifying endpoints for new release of media {mediaName} failed due to {message}",
                        successFullyPersistedScrapeResult?.MediaName, subscribedEndpointsResult.Value.Error);
            }
        }


        private ScrapeInstructionsDTO BuildScrapeInstructionsDTO()
        {
            return new ScrapeInstructionsDTO(
                _trackedMediaSettings.MediaNames
                    .Select(mediaName => new ScrapeInstructionDTO(mediaName.ToLower()))
                    .ToList()
            );
        }

        private AddReleasesDTO BuildAddReleasesDTO(IEnumerable<ScrapeResultDTO> scrapeResults)
        {
            return new AddReleasesDTO(scrapeResults.Select(scrapeResult => new AddReleaseDTO(
                scrapeResult.MediaName.ToLower(),
                scrapeResult.ReleaseNumber,
                scrapeResult.SubReleaseNumber,
                scrapeResult.Url)).ToList());
        }

        private NotificationDTO BuildNotificationDTO(int chapterNumber, int subChapterNumber, string url, List<string> notificationEndpointIdentifiers, string mediaName)
        {
            var message = $"Chapter {chapterNumber.ToString()}";
            message += subChapterNumber > 0
                ? $".{subChapterNumber.ToString()} "
                : " ";
            message += $"is here! Check it out at: {url}";
            return new NotificationDTO(message, notificationEndpointIdentifiers, mediaName);
        }
    }
}