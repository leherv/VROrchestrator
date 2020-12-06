using System.Collections.Generic;

namespace VROrchestrator.DTO.VRNotifier
{
    public class NotificationDTO
    {
        public string Message { get; set; }
        public List<string> NotificationEndpointIdentifiers { get; set; }

        public NotificationDTO(string message, List<string> notificationEndpointIdentifiers)
        {
            Message = message;
            NotificationEndpointIdentifiers = notificationEndpointIdentifiers;
        }
    }
}