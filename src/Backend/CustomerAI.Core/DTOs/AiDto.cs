using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.DTOs
{
    public class AiRequestDto
    {
        public int customer_id { get; set; }
        public string sector { get; set; }
        public int membership_days { get; set; }
        public float total_spend { get; set; }
        public float last_interaction_score { get; set; }
    }

    public class AiResponseDto
    {
        public int customer_id { get; set; }
        public double churn_risk_score { get; set; }
        public string segment { get; set; }
        public string ai_advice { get; set; }
        public string main_reason { get; set; }
    }
}
