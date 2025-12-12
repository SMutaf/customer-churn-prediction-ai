using CustomerAI.Core.Entities.Base;
using CustomerAI.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.Entities
{
    public class Interaction : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public InteractionType Type { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; } 

        public float? SentimentScore { get; set; }
    }
}
