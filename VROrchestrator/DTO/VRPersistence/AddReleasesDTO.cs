using System.Collections.Generic;

namespace VROrchestrator.DTO.VRPersistence
{
    public class AddReleasesDTO
    {
        public List<AddReleaseDTO> Releases { get; set; }

        public AddReleasesDTO(List<AddReleaseDTO> releases)
        {
            Releases = releases;
        }
    }
}