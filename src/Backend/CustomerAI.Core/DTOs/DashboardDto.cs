using System.Collections.Generic;

namespace CustomerAI.Core.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalCustomers { get; set; }       
        public int HighRiskCustomers { get; set; }   
        public double AverageChurnScore { get; set; }  

        public Dictionary<string, int> RiskDistribution { get; set; }
        public List<RecentPredictionDto> RecentPredictions { get; set; }
    }

    public class RecentPredictionDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double ChurnScore { get; set; }
        public string RiskLevel { get; set; } 
        public string RecommendedAction { get; set; }
        public string MainReason { get; set; }
    }
}