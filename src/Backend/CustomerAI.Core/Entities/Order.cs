using CustomerAI.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.Entities
{
    public class Order : BaseEntity
    {
        public int CustomerId {  get; set; }
        public Customer Customer { get; set; }

        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
