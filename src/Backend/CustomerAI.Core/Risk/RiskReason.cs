using CustomerAI.Core.Enums;

namespace CustomerAI.Core.Risk
{
    public class RiskReason
    {
        public RiskCategory Category { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Severity { get; set; }
    }
}
