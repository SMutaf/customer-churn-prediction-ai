using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerAI.Core.DTOs;
using CustomerAI.Core.Entities;
using CustomerAI.Core.Enums; 
using CustomerAI.Data.Context;
using CustomerAI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace CustomerAI.Services.Concrete
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly CustomerAiDbContext _context;
        private readonly IPythonApiService _pythonApiService;

        public AnalyticsService(CustomerAiDbContext context, IPythonApiService pythonApiService)
        {
            _context = context;
            _pythonApiService = pythonApiService;
        }

        public async Task<AiPredictionLog> AnalyzeSingleCustomerAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .Include(c => c.Interactions)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null) throw new Exception("Müşteri bulunamadı!");

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

            var predictionLog = new AiPredictionLog
            {
                CustomerId = customer.Id,
                PredictionDate = DateTime.Now,
                ChurnScore = aiResponse.churn_risk_score,
                RecommendedAction = aiResponse.ai_advice,
                RiskLevel = aiResponse.churn_risk_score > 0.7 ? RiskLevel.High :
                            aiResponse.churn_risk_score > 0.4 ? RiskLevel.Medium : RiskLevel.Low
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
                    Console.WriteLine($"Müşteri ID {id} analiz edilirken hata: {ex.Message}");
                    continue;
                }
            }

            return successCount;
        }
    }
}