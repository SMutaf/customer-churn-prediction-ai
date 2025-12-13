using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerAI.Core.DTOs;
using System.Threading.Tasks;

namespace CustomerAI.Services.Interfaces
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<RiskyCustomerExportDto>> GetRiskyCustomerReportAsync();
    }
}