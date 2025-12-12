using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAI.Core.Enums
{
    public enum RiskLevel
    {
        Low = 1,      // düşük risk
        Medium = 2,   // ilgilenilmesi lazım
        High = 3,     // müşteri kaybedilmek üzere
        Critical = 4  // kritik
    }
}
