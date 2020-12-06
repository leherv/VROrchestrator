namespace VROrchestrator.DTO.VRPersistence
{
    public class AddReleaseDTO
    {
        public string MediaName { get; set; }
        public int ReleaseNumber { get; set; }
        public int SubReleaseNumber { get; set; }
        public string Url { get; set; }

        public AddReleaseDTO(string mediaName, int releaseNumber, int subReleaseNumber, string url)
        {
            MediaName = mediaName;
            ReleaseNumber = releaseNumber;
            SubReleaseNumber = subReleaseNumber;
            Url = url;
        }
    }
}