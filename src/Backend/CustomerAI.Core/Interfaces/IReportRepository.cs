using CustomerAI.Core.DTOs;
using System.Threading.Tasks;

namespace CustomerAI.Core.Interfaces
{
    public interface IReportRepository
    {
        Task<DashboardSummaryDto> GetDashboardDataAsync();
        Task<List<RiskyCustomerExportDto>> GetRiskyCustomersAsync();
    }
}