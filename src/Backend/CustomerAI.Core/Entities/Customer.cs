using CustomerAI.Core.Entities.Base;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone {  get; set; }

        public string Sector { get; set; }
        public string City { get; set; }
        public DateTime MembershipDate { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Interaction> Interactions { get; set; }
        public ICollection<AiPredictionLog> PredictionLogs { get; set; }

        public Customer()
        {
            Orders = new HashSet<Order>();
            Interactions = new HashSet<Interaction>();
            PredictionLogs = new HashSet<AiPredictionLog>();
        }

    }
}
