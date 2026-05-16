using CustomerAI.Core.Enums;
using CustomerAI.Core.Risk;
using CustomerAI.Data.Context;
using CustomerAI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerAI.Services.Concrete
{
    public class CustomerBehaviorService : ICustomerBehaviorService
    {
        private const float NeutralSentimentScore = 3.0f;
        private readonly CustomerAiDbContext _context;

        public CustomerBehaviorService(CustomerAiDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerBehaviorProfile> BuildBehaviorProfileAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .Include(c => c.Interactions)
                .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted);

            if (customer == null)
            {
                throw new Exception("Müşteri bulunamadı!");
            }

            var now = DateTime.Now;
            var activeOrders = customer.Orders.Where(o => !o.IsDeleted).OrderBy(o => o.OrderDate).ToList();
            var activeInteractions = customer.Interactions.Where(i => !i.IsDeleted).OrderBy(i => i.Date).ToList();

            var totalSpend = activeOrders.Sum(o => o.TotalAmount);
            var orderCount = activeOrders.Count;
            var lastOrderDate = activeOrders.LastOrDefault()?.OrderDate;
            var recencyDays = lastOrderDate.HasValue ? Math.Max(0, (now - lastOrderDate.Value).Days) : 999;
            var averageOrderValue = orderCount > 0 ? totalSpend / orderCount : 0;
            var averageOrderGapDays = CalculateAverageOrderGapDays(activeOrders);
            var lastSentiment = activeInteractions.LastOrDefault()?.SentimentScore ?? NeutralSentimentScore;
            var sentimentScores = activeInteractions
                .Where(i => i.SentimentScore.HasValue)
                .Select(i => i.SentimentScore!.Value)
                .ToList();

            var averageSentiment = sentimentScores.Count > 0 ? sentimentScores.Average() : NeutralSentimentScore;
            var spendLast30Days = activeOrders
                .Where(o => o.OrderDate >= now.AddDays(-30))
                .Sum(o => o.TotalAmount);
            var spendLast90Days = activeOrders
                .Where(o => o.OrderDate >= now.AddDays(-90))
                .Sum(o => o.TotalAmount);
            var previous90DaysSpend = activeOrders
                .Where(o => o.OrderDate < now.AddDays(-90) && o.OrderDate >= now.AddDays(-180))
                .Sum(o => o.TotalAmount);

            return new CustomerBehaviorProfile
            {
                CustomerId = customer.Id,
                MembershipDays = Math.Max(0, (now - customer.MembershipDate).Days),
                TotalSpend = totalSpend,
                OrderCount = orderCount,
                LastOrderDate = lastOrderDate,
                RecencyDays = recencyDays,
                AverageOrderValue = averageOrderValue,
                AverageOrderGapDays = averageOrderGapDays,
                LastSentimentScore = lastSentiment,
                AverageSentimentScore = averageSentiment,
                InteractionCount = activeInteractions.Count,
                ComplaintCount = activeInteractions.Count(i => i.Type == InteractionType.Complaint),
                SpendLast30Days = spendLast30Days,
                SpendLast90Days = spendLast90Days,
                Previous90DaysSpend = previous90DaysSpend
            };
        }

        private static double CalculateAverageOrderGapDays(IReadOnlyList<CustomerAI.Core.Entities.Order> orders)
        {
            if (orders.Count < 2)
            {
                return 0;
            }

            var gaps = new List<double>();
            for (var i = 1; i < orders.Count; i++)
            {
                gaps.Add(Math.Max(0, (orders[i].OrderDate - orders[i - 1].OrderDate).TotalDays));
            }

            return Math.Round(gaps.Average(), 2);
        }
    }
}
