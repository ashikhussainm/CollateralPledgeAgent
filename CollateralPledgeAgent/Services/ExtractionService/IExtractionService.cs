using CollateralPledgeAgent.Models;

namespace CollateralPledgeAgent.Services
{
    public interface IExtractionService
    {
        Task<IList<CollateralRow>> ExtractCollateralRowsAsync(string rawText);
    }
}