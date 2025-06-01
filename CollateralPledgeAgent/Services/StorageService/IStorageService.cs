
namespace CollateralPledgeAgent.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName);
    }
}