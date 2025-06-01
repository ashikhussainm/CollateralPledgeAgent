using CollateralPledgeAgent.Models;

namespace CollateralPledgeAgent.Services
{
    public interface ISheetsService
    {
        Task AppendRowsAsync(IList<CollateralRow> rows);
    }
}