using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VRNotifier.Extensions;
using VROrchestrator.DTO;

namespace VROrchestrator.HttpClients
{
    public interface IVRScraperClient
    {
        Task<Result<List<SerializableResult<ScrapeResultDTO>>>> Scrape(
            ScrapeInstructionsDTO scrapeInstructionsDto);
    }
}