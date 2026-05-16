using CustomerAI.Core.DTOs;
using CustomerAI.Core.Interfaces; 
using CustomerAI.Services.Interfaces;
using System.Threading.Tasks;

namespace CustomerAI.Services.Concrete
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var data = await _reportRepository.GetDashboardDataAsync();

            return data;
        }

        public async Task<List<RiskyCustomerExportDto>> GetRiskyCustomerReportAsync()
        {
            var report = await _reportRepository.GetRiskyCustomersAsync();

            return report;
        }
    }
}
