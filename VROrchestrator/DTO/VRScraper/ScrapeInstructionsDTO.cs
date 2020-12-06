using System.Collections.Generic;

namespace VROrchestrator.DTO
{
    public class ScrapeInstructionsDTO
    {
        public List<ScrapeInstructionDTO> ScrapeInstructions { get; set; }

        public ScrapeInstructionsDTO(List<ScrapeInstructionDTO> scrapeInstructions)
        {
            ScrapeInstructions = scrapeInstructions;
        }
    }
}