using CustomerAI.Core.DTOs;
using CustomerAI.Core.Enums;
using CustomerAI.Core.Interfaces;
using CustomerAI.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAI.Data.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly CustomerAiDbContext _context;

        public ReportRepository(CustomerAiDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardDataAsync()
        {
            var totalCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted);

            // veriyi belleğe çek
            var latestPredictionsQuery = _context.AiPredictionLogs
                .GroupBy(l => l.CustomerId)
                .Select(g => g.OrderByDescending(l => l.PredictionDate).FirstOrDefault());

            var latestPredictions = await latestPredictionsQuery.ToListAsync();

            var highRiskCount = latestPredictions.Count(l => l.RiskLevel == RiskLevel.High || l.RiskLevel == RiskLevel.Critical);

            double avgScore = 0;
            if (latestPredictions.Any())
            {
                avgScore = latestPredictions.Average(l => l.ChurnScore);
            }

            var distribution = latestPredictions
                .GroupBy(l => l.RiskLevel)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var recentList = await _context.AiPredictionLogs
                .Include(l => l.Customer)
                .OrderByDescending(l => l.PredictionDate)
                .Take(5)
                .Select(l => new RecentPredictionDto
                {
                    CustomerId = l.CustomerId,
                    CustomerName = l.Customer.Name,
                    ChurnScore = l.ChurnScore,
                    RiskLevel = l.RiskLevel.ToString(),
                    RecommendedAction = l.RecommendedAction,
                    MainReason = l.MainReason
                })
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalCustomers = totalCustomers,
                HighRiskCustomers = highRiskCount,
                AverageChurnScore = Math.Round(avgScore, 2),
                RiskDistribution = distribution,
                RecentPredictions = recentList
            };
        }

        public async Task<List<RiskyCustomerExportDto>> GetRiskyCustomersAsync()
        {
            var rawLogs = await _context.AiPredictionLogs
                .Include(c => c.Customer)
                .Where(x => x.RiskLevel == Core.Enums.RiskLevel.High || x.RiskLevel == Core.Enums.RiskLevel.Critical)
                .ToListAsync(); 

            var riskyList = rawLogs
                .GroupBy(x => x.CustomerId) 
                .Select(g => g.OrderByDescending(x => x.PredictionDate).FirstOrDefault()) 
                .Select(x => new RiskyCustomerExportDto 
                {
                    CustomerName = x.Customer.Name,
                    Email = x.Customer.Email,
                    Phone = x.Customer.Phone,
                    ChurnScore = x.ChurnScore,
                    RiskLevel = x.RiskLevel.ToString(),
                    RecommendedAction = x.RecommendedAction,
                    MainReason = x.MainReason
                })
                .ToList();

            return riskyList;
        }
    }
}