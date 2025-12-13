using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.DTOs
{
    public class RiskyCustomerExportDto
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public double ChurnScore { get; set; }
        public string RiskLevel { get; set; }
        public string RecommendedAction { get; set; }
    }
}
