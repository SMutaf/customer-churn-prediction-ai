using CustomerAI.API.Hubs;
using CustomerAI.Core.Entities;
using CustomerAI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CustomerAI.API.Realtime
{
    public class SignalRAnalyticsRealtimeNotifier : IAnalyticsRealtimeNotifier
    {
        private readonly IHubContext<AnalyticsHub> _hubContext;

        public SignalRAnalyticsRealtimeNotifier(IHubContext<AnalyticsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task AnalysisStartedAsync(int totalCustomers)
        {
            return _hubContext.Clients.All.SendAsync("analysisStarted", new
            {
                totalCustomers,
                analyzedCustomers = 0,
                date = DateTime.Now
            });
        }

        public Task CustomerAnalyzedAsync(AiPredictionLog prediction, int analyzedCustomers, int totalCustomers)
        {
            return _hubContext.Clients.All.SendAsync("customerAnalyzed", new
            {
                id = prediction.Id,
                customerId = prediction.CustomerId,
                churnScore = prediction.ChurnScore,
                coreRiskScore = prediction.CoreRiskScore,
                mlChurnProbability = prediction.MlChurnProbability,
                finalRiskScore = prediction.FinalRiskScore,
                riskLevel = prediction.RiskLevel.ToString(),
                segment = prediction.Segment,
                recommendedAction = prediction.RecommendedAction,
                mainReason = prediction.MainReason,
                modelExplanationsJson = prediction.ModelExplanationsJson,
                predictionDate = prediction.PredictionDate,
                analyzedCustomers,
                totalCustomers
            });
        }

        public Task CustomerAnalysisFailedAsync(int customerId, string message, int analyzedCustomers, int totalCustomers)
        {
            return _hubContext.Clients.All.SendAsync("customerAnalysisFailed", new
            {
                customerId,
                message,
                analyzedCustomers,
                totalCustomers,
                date = DateTime.Now
            });
        }

        public Task AnalysisCompletedAsync(int analyzedCustomers, int totalCustomers)
        {
            return _hubContext.Clients.All.SendAsync("analysisCompleted", new
            {
                analyzedCustomers,
                totalCustomers,
                date = DateTime.Now
            });
        }
    }
}
