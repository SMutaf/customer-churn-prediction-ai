using CustomerAI.Core.Entities.Base;
using CustomerAI.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.Entities
{
    public class AiPredictionLog : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime PredictionDate { get; set; } = DateTime.Now;

        // 0.0 ile 1.0 dönücülek risk skoru
        public double ChurnScore { get; set; }

        // skorun yorumlanması
        public RiskLevel RiskLevel { get; set; }

        // AI aracılığıyla alınması gereken aksiyon
        public string RecommendedAction { get; set; }
    }
}
