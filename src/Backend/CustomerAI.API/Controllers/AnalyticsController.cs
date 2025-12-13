using CustomerAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // POST api/analytics/analyze/1
        [HttpPost("analyze/{customerId}")]
        public async Task<IActionResult> AnalyzeCustomer(int customerId)
        {
            try
            {
                var result = await _analyticsService.AnalyzeSingleCustomerAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("analyze-all")]
        public async Task<IActionResult> AnalyzeAllCustomers()
        {
            try
            {
                // İşlemi başlat
                int count = await _analyticsService.AnalyzeAllCustomersAsync();

                return Ok(new
                {
                    message = "Toplu analiz başarıyla tamamlandı.",
                    analyzed_customers = count,
                    date = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}