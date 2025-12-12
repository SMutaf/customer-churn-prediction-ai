using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerAI.Core.Entities;
using System.Threading.Tasks;

namespace CustomerAI.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AiPredictionLog> AnalyzeSingleCustomerAsync(int customerId);

        Task<int> AnalyzeAllCustomersAsync();
    }
}