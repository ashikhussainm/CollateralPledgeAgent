using CollateralPledgeAgent.Models;

namespace CollateralPledgeAgent.Services
{
    public class ExtractionService : IExtractionService
    {
        public ExtractionService() { }
        /// <summary>
        /// Extracts collateral rows from the provided raw text.
        /// </summary>
        /// <param name="rawText"></param>
        /// <returns></returns>
        public async Task<IList<CollateralRow>> ExtractCollateralRowsAsync(string rawText)
        {
            // ── STUB IMPLEMENTATION ───────────────────────────────────────────────────
            // For now, return a dummy row. In a real POC, replace this by:
            //  1) Semantic Kernel prompt to an LLM: “Extract JSON array of collateral rows…”
            //  2) Regex-based parser if rawText is a consistent table.
            var dummy = new CollateralRow
            {
                MemberId = "Member123",
                ReportDate = DateTime.UtcNow.Date,
                Cusip = "123456789",
                CollateralType = "FNMA MBS",
                MarketValue = 1000000m,
                HaircutPercent = 0.08m,
                EligibleAdvanceAmount = 920000m
            };
            return new List<CollateralRow> { dummy };
        }
    }
}
