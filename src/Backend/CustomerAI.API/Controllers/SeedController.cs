using CustomerAI.Data.Context;
using CustomerAI.Data.Seeds;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly CustomerAiDbContext _context;

        public SeedController(CustomerAiDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate-fake-data")]
        public async Task<IActionResult> GenerateFakeData([FromQuery] int count = 1000)
        {
            var seeder = new DataSeeder(_context);
            var addedCount = await seeder.SeedAsync(count);
            return Ok(new
            {
                target_customers = Math.Max(count, 500),
                added_customers = addedCount,
                message = $"{addedCount} adet sahte müşteri başarıyla oluşturuldu."
            });
        }
    }
}
