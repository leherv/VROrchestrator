using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VRNotifier.Extensions;
using VROrchestrator.DTO.VRPersistence;

namespace VRNotifier.Services.VRPersistence
{
    public interface IVRPersistenceClient
    {
        Task<Result<List<SerializableResult>>> AddReleases(AddReleasesDTO addReleasesDto);
        Task<Result<SerializableResult<List<NotificationEndpointDTO>>>> GetSubscribedEndpoints(string mediaName);
    }
}