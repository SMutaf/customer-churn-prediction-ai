using CustomerAI.Core.DTOs;
using CustomerAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IPythonApiService _pythonApiService; 

        public CustomersController(ICustomerService customerService, IPythonApiService pythonApiService)
        {
            _customerService = customerService;
            _pythonApiService = pythonApiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerDto customerDto)
        {
            var result = await _customerService.AddAsync(customerDto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("test-ai-connection")]
        public async Task<IActionResult> TestAiConnection()
        {
            // test verisi
            var dummyData = new AiRequestDto
            {
                customer_id = 999,     
                sector = "Teknoloji",
                membership_days = 500, 
                total_spend = 100,     
                last_interaction_score = -0.8f 
            };

            var result = await _pythonApiService.GetChurnPredictionAsync(dummyData);

            return Ok(result);
        }
    }
}