namespace VROrchestrator.DTO
{
    public class ScrapeInstructionDTO
    {
        public string MediaName { get; set; }

        public ScrapeInstructionDTO(string mediaName)
        {
            MediaName = mediaName;
        }
    }
}