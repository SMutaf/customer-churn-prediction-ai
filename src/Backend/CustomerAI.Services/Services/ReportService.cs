using CustomerAI.Core.DTOs;
using CustomerAI.Core.Interfaces; 
using CustomerAI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CustomerAI.Services.Concrete
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository reportRepository, ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            _logger.LogInformation("Dashboard özet verileri talep edildi.");

            var data = await _reportRepository.GetDashboardDataAsync();

            _logger.LogInformation("Dashboard verileri başarıyla çekildi. Toplam Müşteri: {Total}, Riskli: {Risky}",
                data.TotalCustomers, data.HighRiskCustomers);

            return data;
        }

        public async Task<List<RiskyCustomerExportDto>> GetRiskyCustomerReportAsync()
        {
            _logger.LogInformation("Riskli müşteri raporu (Excel/CSV) için veriler hazırlanıyor...");

            var report = await _reportRepository.GetRiskyCustomersAsync();

            if (report == null || report.Count == 0)
            {
                _logger.LogWarning("Riskli müşteri raporu BOŞ döndü. Şu an riskli müşteri yok veya bir sorun var.");
            }
            else
            {
                _logger.LogInformation("Rapor başarıyla hazırlandı. Listeye {Count} adet riskli müşteri eklendi.", report.Count);
            }

            return report;
        }
    }
}