using CustomerAI.Core.DTOs;
using CustomerAI.Core.Entities;
using CustomerAI.Core.Enums;
using CustomerAI.Core.Risk;
using CustomerAI.Data.Context;
using CustomerAI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CustomerAI.Services.Concrete
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly CustomerAiDbContext _context;
        private readonly ICustomerBehaviorService _customerBehaviorService;
        private readonly IFeatureExtractionService _featureExtractionService;
        private readonly ICoreRiskEngine _coreRiskEngine;
        private readonly IPythonApiService _pythonApiService;
        private readonly IFinalRiskDecisionService _finalRiskDecisionService;
        private readonly ISegmentAssignmentService _segmentAssignmentService;

        public AnalyticsService(
            CustomerAiDbContext context,
            ICustomerBehaviorService customerBehaviorService,
            IFeatureExtractionService featureExtractionService,
            ICoreRiskEngine coreRiskEngine,
            IPythonApiService pythonApiService,
            IFinalRiskDecisionService finalRiskDecisionService,
            ISegmentAssignmentService segmentAssignmentService)
        {
            _context = context;
            _customerBehaviorService = customerBehaviorService;
            _featureExtractionService = featureExtractionService;
            _coreRiskEngine = coreRiskEngine;
            _pythonApiService = pythonApiService;
            _finalRiskDecisionService = finalRiskDecisionService;
            _segmentAssignmentService = segmentAssignmentService;
        }

        public async Task<AiPredictionLog> AnalyzeSingleCustomerAsync(int customerId)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted);

            if (customer == null)
            {
                throw new Exception("Müşteri bulunamadı!");
            }

            var behaviorProfile = await _customerBehaviorService.BuildBehaviorProfileAsync(customerId);
            var featureVector = _featureExtractionService.Extract(behaviorProfile);
            var coreRiskProfile = _coreRiskEngine.Evaluate(featureVector);
            var aiRequest = BuildAiRequest(customer.Sector, featureVector);
            var aiResponse = await _pythonApiService.GetChurnPredictionAsync(aiRequest);
            var finalProfile = _finalRiskDecisionService.BuildFinalProfile(coreRiskProfile, aiResponse);
            finalProfile.Segment = _segmentAssignmentService.Assign(finalProfile, featureVector);

            var predictionLog = new AiPredictionLog
            {
                CustomerId = finalProfile.CustomerId,
                PredictionDate = finalProfile.CreatedAt,
                ChurnScore = finalProfile.FinalRiskScore / 100,
                CoreRiskScore = finalProfile.CoreRiskScore,
                MlChurnProbability = finalProfile.MlChurnProbability,
                FinalRiskScore = finalProfile.FinalRiskScore,
                RecommendedAction = finalProfile.RecommendedAction,
                RiskLevel = finalProfile.RiskLevel,
                Segment = finalProfile.Segment.ToString(),
                MainReason = finalProfile.MainReason,
                TriggeredRulesJson = JsonSerializer.Serialize(finalProfile.TriggeredRules),
                ModelExplanationsJson = JsonSerializer.Serialize(aiResponse.model_explanations ?? new ModelExplanationDto { Method = "unavailable" })
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

            var successCount = 0;

            foreach (var id in allCustomerIds)
            {
                try
                {
                    await AnalyzeSingleCustomerAsync(id);
                    successCount++;
                }
                catch
                {
                }
            }

            return successCount;
        }

        private static AiRequestDto BuildAiRequest(string sector, CustomerFeatureVector featureVector)
        {
            return new AiRequestDto
            {
                customer_id = featureVector.CustomerId,
                sector = sector,
                total_spend = featureVector.TotalSpend,
                membership_days = featureVector.MembershipDays,
                recency_days = featureVector.RecencyDays,
                order_count = featureVector.OrderCount,
                average_order_value = featureVector.AverageOrderValue,
                average_order_gap_days = featureVector.AverageOrderGapDays,
                purchase_frequency = featureVector.PurchaseFrequency,
                last_interaction_score = featureVector.LastSentimentScore,
                average_sentiment_score = featureVector.AverageSentimentScore,
                interaction_count = featureVector.InteractionCount,
                complaint_count = featureVector.ComplaintCount,
                spend_last_30_days = featureVector.SpendLast30Days,
                spend_last_90_days = featureVector.SpendLast90Days,
                spend_drop_rate = featureVector.SpendDropRate
            };
        }

    }
}
