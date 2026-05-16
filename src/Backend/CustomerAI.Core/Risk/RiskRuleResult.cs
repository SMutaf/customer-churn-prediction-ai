using CustomerAI.Core.Enums;

namespace CustomerAI.Core.Risk
{
    public class RiskRuleResult
    {
        public RiskCategory Category { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public double ScoreImpact { get; set; }
        public int Severity { get; set; }
    }
}
