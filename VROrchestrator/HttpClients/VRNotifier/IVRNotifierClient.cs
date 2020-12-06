using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VRNotifier.Extensions;
using VROrchestrator.DTO.VRNotifier;

namespace VROrchestrator.HttpClients.VRNotifier
{
    public interface IVRNotifierClient
    {
        Task<Result<SerializableResult>> Notify(NotificationDTO notificationDto);
    }
}