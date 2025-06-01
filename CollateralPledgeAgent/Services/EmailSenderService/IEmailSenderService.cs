
namespace CollateralPledgeAgent.Services
{
    public interface IEmailSenderService
    {
        Task SendConfirmationAsync(string toEmail, string subject, string bodyHtml, IList<string>? cc = null);
    }
}