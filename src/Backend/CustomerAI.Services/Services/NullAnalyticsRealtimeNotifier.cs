using CustomerAI.Core.Entities;
using CustomerAI.Services.Interfaces;

namespace CustomerAI.Services.Concrete
{
    public class NullAnalyticsRealtimeNotifier : IAnalyticsRealtimeNotifier
    {
        public Task AnalysisStartedAsync(int totalCustomers) => Task.CompletedTask;

        public Task CustomerAnalyzedAsync(AiPredictionLog prediction, int analyzedCustomers, int totalCustomers) => Task.CompletedTask;

        public Task CustomerAnalysisFailedAsync(int customerId, string message, int analyzedCustomers, int totalCustomers) => Task.CompletedTask;

        public Task AnalysisCompletedAsync(int analyzedCustomers, int totalCustomers) => Task.CompletedTask;
    }
}
