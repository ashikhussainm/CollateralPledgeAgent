
namespace CollateralPledgeAgent.Services
{
    public interface IOcrService
    {
        Task<string> ExtractRawTextAsync(byte[] fileBytes);
    }
}