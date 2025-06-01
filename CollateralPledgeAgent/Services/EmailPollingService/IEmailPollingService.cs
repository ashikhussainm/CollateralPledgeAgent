using Microsoft.Extensions.Hosting;

namespace CollateralPledgeAgent.Services
{
    public interface IEmailPollingService : IHostedService
    {
        // BackgroundService implements IHostedService, so no extra members here.
    }
}
