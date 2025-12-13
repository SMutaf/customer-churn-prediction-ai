using CustomerAI.Core.DTOs;
using CustomerAI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json; 
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomerAI.Services.Concrete
{
    public class PythonApiService : IPythonApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PythonApiService> _logger;


        public PythonApiService(HttpClient httpClient, ILogger<PythonApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AiResponseDto> GetChurnPredictionAsync(AiRequestDto request)
        {
            _logger.LogInformation("Python AI servisine tahmin isteği gönderiliyor. CustomerID: {CustomerId}, Sektör: {Sector}", request.customer_id, request.sector);

            // python adres: http://localhost:5000/predict/churn
            var response = await _httpClient.PostAsJsonAsync("/predict/churn", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Python API Hatası! Durum Kodu: {StatusCode}. Python Mesajı: {ErrorContent}", response.StatusCode, errorContent);

                response.EnsureSuccessStatusCode();
            }

            var result = await response.Content.ReadFromJsonAsync<AiResponseDto>();

            _logger.LogInformation("Python'dan tahmin başarıyla alındı. Churn Skoru: {Score}, Öneri: {Advice}", result.churn_risk_score, result.ai_advice);

            return result;
        }
    }
}