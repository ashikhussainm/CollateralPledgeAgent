using Microsoft.AspNetCore.Mvc;

namespace CollateralPledgeAgent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "CollateralPledgeAgent is running." });
    }
}
