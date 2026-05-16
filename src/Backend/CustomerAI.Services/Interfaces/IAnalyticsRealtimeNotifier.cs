using CustomerAI.Core.Entities;

namespace CustomerAI.Services.Interfaces
{
    public interface IAnalyticsRealtimeNotifier
    {
        Task AnalysisStartedAsync(int totalCustomers);
        Task CustomerAnalyzedAsync(AiPredictionLog prediction, int analyzedCustomers, int totalCustomers);
        Task CustomerAnalysisFailedAsync(int customerId, string message, int analyzedCustomers, int totalCustomers);
        Task AnalysisCompletedAsync(int analyzedCustomers, int totalCustomers);
    }
}
