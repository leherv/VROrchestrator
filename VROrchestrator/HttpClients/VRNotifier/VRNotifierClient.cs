using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VRNotifier.Extensions;
using VRNotifier.Services;
using VROrchestrator.DTO.VRNotifier;
using VROrchestrator.Extensions;

namespace VROrchestrator.HttpClients.VRNotifier
{
    public class VRNotifierClient : IVRNotifierClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<VRNotifierClient> _logger;

        public VRNotifierClient(HttpClient client, ILogger<VRNotifierClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<SerializableResult>> Notify(NotificationDTO notificationDto)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, "notification");
            message.Content = new StringContent(JsonHandler.Serialize(notificationDto), Encoding.UTF8,
                "application/json");
            var response = await message.SendRequest(_client);
            if (response.IsFailure)
            {
                return Result.Failure<SerializableResult>("Call to VRScraper failed.");
            }

            var deserializeResult =
                JsonHandler.Deserialize<SerializableResult>(await response.Value.Content.ReadAsStringAsync());
            if (deserializeResult.IsFailure)
            {
                _logger.LogError("Deserialization failed due to: {errorDetails}.", deserializeResult.Error);
                return Result.Failure<SerializableResult>("Deserialization failed.");
            }
            return Result.Success(deserializeResult.Value);
        }
    }
}