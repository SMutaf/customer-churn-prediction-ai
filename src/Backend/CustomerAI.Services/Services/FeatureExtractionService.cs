using CustomerAI.Core.Risk;
using CustomerAI.Services.Interfaces;

namespace CustomerAI.Services.Concrete
{
    public class FeatureExtractionService : IFeatureExtractionService
    {
        public CustomerFeatureVector Extract(CustomerBehaviorProfile behaviorProfile)
        {
            var purchaseFrequency = behaviorProfile.MembershipDays > 0
                ? (double)behaviorProfile.OrderCount / behaviorProfile.MembershipDays
                : 0;

            var spendDropRate = behaviorProfile.Previous90DaysSpend > 0
                ? (double)((behaviorProfile.SpendLast90Days - behaviorProfile.Previous90DaysSpend) / behaviorProfile.Previous90DaysSpend)
                : 0;

            return new CustomerFeatureVector
            {
                CustomerId = behaviorProfile.CustomerId,
                TotalSpend = (double)behaviorProfile.TotalSpend,
                MembershipDays = behaviorProfile.MembershipDays,
                RecencyDays = behaviorProfile.RecencyDays,
                OrderCount = behaviorProfile.OrderCount,
                AverageOrderValue = (double)behaviorProfile.AverageOrderValue,
                AverageOrderGapDays = behaviorProfile.AverageOrderGapDays,
                PurchaseFrequency = Math.Round(purchaseFrequency, 6),
                LastSentimentScore = behaviorProfile.LastSentimentScore,
                AverageSentimentScore = behaviorProfile.AverageSentimentScore,
                InteractionCount = behaviorProfile.InteractionCount,
                ComplaintCount = behaviorProfile.ComplaintCount,
                SpendLast30Days = (double)behaviorProfile.SpendLast30Days,
                SpendLast90Days = (double)behaviorProfile.SpendLast90Days,
                SpendDropRate = Math.Round(spendDropRate, 4)
            };
        }
    }
}
