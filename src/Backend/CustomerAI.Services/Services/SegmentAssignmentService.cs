using CustomerAI.Core.Enums;
using CustomerAI.Core.Risk;
using CustomerAI.Services.Interfaces;

namespace CustomerAI.Services.Concrete
{
    public class SegmentAssignmentService : ISegmentAssignmentService
    {
        private const double HighValueSpend = 10000;
        private const double LowRevenueSpend = 1000;

        public SegmentType Assign(CustomerRiskProfile riskProfile, CustomerFeatureVector featureVector)
        {
            if (featureVector.TotalSpend >= HighValueSpend && riskProfile.RiskLevel == RiskLevel.Low)
            {
                return SegmentType.VipLoyal;
            }

            if (featureVector.TotalSpend >= HighValueSpend && featureVector.LastSentimentScore < 2.5)
            {
                return SegmentType.AngryHighValue;
            }

            if (featureVector.RecencyDays > 180)
            {
                return SegmentType.DormantCustomer;
            }

            if (featureVector.InteractionCount == 0 && riskProfile.FinalRiskScore >= 50)
            {
                return SegmentType.SilentChurnRisk;
            }

            if (featureVector.SpendDropRate <= -0.5)
            {
                return SegmentType.PriceSensitive;
            }

            if (featureVector.OrderCount >= 3 && featureVector.AverageOrderValue < 500)
            {
                return SegmentType.DiscountHunter;
            }

            if (featureVector.InteractionCount >= 4 && featureVector.TotalSpend < LowRevenueSpend)
            {
                return SegmentType.HighEngagementLowRevenue;
            }

            return SegmentType.StandardCustomer;
        }
    }
}
