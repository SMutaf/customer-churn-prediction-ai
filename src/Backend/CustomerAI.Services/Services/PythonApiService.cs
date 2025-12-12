using CustomerAI.Core.DTOs;
using CustomerAI.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json; 
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomerAI.Services.Concrete
{
    public class PythonApiService : IPythonApiService
    {
        private readonly HttpClient _httpClient;


        public PythonApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AiResponseDto> GetChurnPredictionAsync(AiRequestDto request)
        {
            // python adres: http://localhost:5000/predict/churn
            var response = await _httpClient.PostAsJsonAsync("/predict/churn", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AiResponseDto>();

            return result;
        }
    }
}