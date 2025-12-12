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
            return await _reportRepository.GetDashboardDataAsync();
        }
    }
}