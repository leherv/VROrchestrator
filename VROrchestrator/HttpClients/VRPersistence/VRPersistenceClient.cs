using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VRNotifier.Extensions;
using VRNotifier.Services;
using VRNotifier.Services.VRPersistence;
using VROrchestrator.DTO.VRPersistence;
using VROrchestrator.Extensions;

namespace VROrchestrator.HttpClients.VRPersistence
{
    public class VRPersistenceClient : IVRPersistenceClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<VRPersistenceClient> _logger;

        public VRPersistenceClient(HttpClient client, ILogger<VRPersistenceClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<List<SerializableResult>>> AddReleases(AddReleasesDTO addReleasesDto)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, "release");
            message.Content = new StringContent(JsonHandler.Serialize(addReleasesDto), Encoding.UTF8,
                "application/json");
            var response = await message.SendRequest(_client);
            if (response.IsFailure)
            {
                return Result.Failure<List<SerializableResult>>("Call to VRScraper failed.");
            }

            var deserializeResult =
                JsonHandler.Deserialize<List<SerializableResult>>(await response.Value.Content.ReadAsStringAsync());
            if (deserializeResult.IsFailure)
            {
                _logger.LogError("Deserialization failed due to: {errorDetails}.", deserializeResult.Error);
                return Result.Failure<List<SerializableResult>>("Deserialization failed.");
            }
            return Result.Success(deserializeResult.Value);
        }

        public async Task<Result<SerializableResult<List<NotificationEndpointDTO>>>> GetSubscribedEndpoints(string mediaName) 
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"notificationendpoint/{mediaName}");
            var response = await message.SendRequest(_client);
            if (response.IsFailure)
            {
                return Result.Failure<SerializableResult<List<NotificationEndpointDTO>>>("Call to VRScraper failed.");
            }

            var deserializeResult =
                JsonHandler.Deserialize<SerializableResult<List<NotificationEndpointDTO>>>(await response.Value.Content.ReadAsStringAsync());
            if (deserializeResult.IsFailure)
            {
                _logger.LogError("Deserialization failed due to: {errorDetails}.", deserializeResult.Error);
                return Result.Failure<SerializableResult<List<NotificationEndpointDTO>>>("Deserialization failed.");
            }
            return Result.Success(deserializeResult.Value);
        }
    }
}