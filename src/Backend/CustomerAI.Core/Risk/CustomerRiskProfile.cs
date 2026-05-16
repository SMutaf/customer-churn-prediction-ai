using CustomerAI.Core.Enums;

namespace CustomerAI.Core.Risk
{
    public class CustomerRiskProfile
    {
        public int CustomerId { get; set; }
        public double CoreRiskScore { get; set; }
        public double MlChurnProbability { get; set; }
        public double FinalRiskScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public SegmentType Segment { get; set; }
        public string MainReason { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public List<RiskRuleResult> TriggeredRules { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Dictionary<string, double> ModelExplanations { get; set; } = new();
    }
}
