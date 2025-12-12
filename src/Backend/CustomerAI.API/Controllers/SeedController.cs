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
        public async Task<IActionResult> GenerateFakeData()
        {
            var seeder = new DataSeeder(_context);
            await seeder.SeedAsync(1000); // 1000 Müşteri oluştur
            return Ok("1000 Adet sahte veri başarıyla oluşturuldu! 🚀");
        }
    }
}