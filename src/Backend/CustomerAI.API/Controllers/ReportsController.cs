using CustomerAI.Services.Concrete;
using CustomerAI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _reportService.GetDashboardSummaryAsync();
            return Ok(stats);
        }


        [HttpGet("export-risky-customers")]
        public async Task<IActionResult> ExportRiskyCustomers()
        {
            var result = await _reportService.GetRiskyCustomerReportAsync();
            return Ok(result);
        }

    }
}
