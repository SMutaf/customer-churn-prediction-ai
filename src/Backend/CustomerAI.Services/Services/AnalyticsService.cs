using Azure;
using CustomerAI.Core.DTOs;
using CustomerAI.Core.Entities;
using CustomerAI.Core.Enums;
using CustomerAI.Core.Interfaces;
using CustomerAI.Data.Context;
using CustomerAI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace CustomerAI.Services.Concrete
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly CustomerAiDbContext _context;
        private readonly IPythonApiService _pythonApiService;
        private readonly ILogger _logger;

        public AnalyticsService(CustomerAiDbContext context, IPythonApiService pythonApiService, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _pythonApiService = pythonApiService;
            _logger = logger;
        }

        public async Task<AiPredictionLog> AnalyzeSingleCustomerAsync(int customerId)
        {
            _logger.LogInformation("Müşteri analizi başlatılıyor. ID: {CustomerId}", customerId);

            var customer = await _context.Customers
                .Include(c => c.Orders)
                .Include(c => c.Interactions)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
            {
                _logger.LogWarning("Analiz iptal edildi: Müşteri bulunamadı! ID: {CustomerId}", customerId);
                throw new Exception("Müşteri bulunamadı!");
            }

            int membershipDays = (DateTime.Now - customer.MembershipDate).Days;
            float totalSpend = (float)customer.Orders.Sum(o => o.TotalAmount);

            float lastSentiment = 0;
            var lastInteraction = customer.Interactions.OrderByDescending(i => i.Date).FirstOrDefault();
            if (lastInteraction != null && lastInteraction.SentimentScore.HasValue)
            {
                lastSentiment = lastInteraction.SentimentScore.Value;
            }

            var aiRequest = new AiRequestDto
            {
                customer_id = customer.Id,
                sector = customer.Sector,
                membership_days = membershipDays,
                total_spend = totalSpend,
                last_interaction_score = lastSentiment
            };

            var aiResponse = await _pythonApiService.GetChurnPredictionAsync(aiRequest);

            var riskLevel = aiResponse.churn_risk_score > 0.7 ? RiskLevel.High :
                            aiResponse.churn_risk_score > 0.4 ? RiskLevel.Medium : RiskLevel.Low;

            if (riskLevel == RiskLevel.High)
            {
                _logger.LogWarning("DİKKAT! Yüksek Riskli Müşteri Tespit Edildi! ID: {Id}, Skor: {Score}", customer.Id, aiResponse.churn_risk_score);
            }
            // daha sonra sill
            if (string.IsNullOrEmpty(aiResponse.ai_advice))
                throw new Exception("AI Advice boş geldi");

            var predictionLog = new AiPredictionLog
            {
                CustomerId = customer.Id,
                PredictionDate = DateTime.Now,
                ChurnScore = aiResponse.churn_risk_score,
                RecommendedAction = aiResponse.ai_advice,
                RiskLevel = aiResponse.churn_risk_score > 0.7 ? RiskLevel.High :
                            aiResponse.churn_risk_score > 0.4 ? RiskLevel.Medium : RiskLevel.Low,
                MainReason = aiResponse.main_reason
            };

            await _context.AiPredictionLogs.AddAsync(predictionLog);
            await _context.SaveChangesAsync();

            return predictionLog;
        }

        public async Task<int> AnalyzeAllCustomersAsync()
        {
            var allCustomerIds = await _context.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            _logger.LogInformation("Toplu analiz başladı. Toplam {Count} müşteri taranacak.", allCustomerIds.Count);

            int successCount = 0;

            foreach (var id in allCustomerIds)
            {
                try
                {
                    await AnalyzeSingleCustomerAsync(id);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Toplu işlem sırasında Müşteri ID: {Id} analiz edilemedi.", id);
                    continue;
                }
            }

            _logger.LogInformation("Toplu analiz bitti. {Success}/{Total} başarı oranı.", successCount, allCustomerIds.Count);
            return successCount;
        }
   
    }
}