namespace CollateralPledgeAgent.Models
{
    /// <summary>
    /// Class representing a row of collateral data extracted from a report.
    /// </summary>
    public class CollateralRow
    {
        public string MemberId { get; set; }
        public DateTime ReportDate { get; set; }
        public string Cusip { get; set; }
        public string CollateralType { get; set; }
        public decimal MarketValue { get; set; }
        public decimal HaircutPercent { get; set; }
        public decimal EligibleAdvanceAmount { get; set; }
    }
}
