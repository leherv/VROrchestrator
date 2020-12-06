using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VRNotifier.Extensions;
using VRNotifier.Services;
using VROrchestrator.DTO;
using VROrchestrator.Extensions;

namespace VROrchestrator.HttpClients.VRScraper
{
    public class VRScraperClient : IVRScraperClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<VRScraperClient> _logger;

        public VRScraperClient(ILogger<VRScraperClient> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<Result<List<SerializableResult<ScrapeResultDTO>>>> Scrape(ScrapeInstructionsDTO scrapeInstructionsDto)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, "scrape");
            message.Content = new StringContent(JsonHandler.Serialize(scrapeInstructionsDto), Encoding.UTF8,
                "application/json");
            var response = await message.SendRequest(_client);
            if (response.IsFailure)
            {
                return Result.Failure<List<SerializableResult<ScrapeResultDTO>>>("Call to VRScraper failed.");
            }

            var deserializeResult =
                JsonHandler.Deserialize<List<SerializableResult<ScrapeResultDTO>>>(await response.Value.Content.ReadAsStringAsync());
            if (deserializeResult.IsFailure)
            {
                _logger.LogError("Deserialization failed due to: {errorDetails}.", deserializeResult.Error);
                return Result.Failure<List<SerializableResult<ScrapeResultDTO>>>("Deserialization failed.");
            }
            return Result.Success(deserializeResult.Value);
        }
    }
}