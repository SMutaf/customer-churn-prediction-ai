namespace CustomerAI.Core.Risk
{
    public class CustomerBehaviorProfile
    {
        public int CustomerId { get; set; }
        public int MembershipDays { get; set; }
        public decimal TotalSpend { get; set; }
        public int OrderCount { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int RecencyDays { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double AverageOrderGapDays { get; set; }
        public float LastSentimentScore { get; set; }
        public float AverageSentimentScore { get; set; }
        public int InteractionCount { get; set; }
        public int ComplaintCount { get; set; }
        public decimal SpendLast30Days { get; set; }
        public decimal SpendLast90Days { get; set; }
        public decimal Previous90DaysSpend { get; set; }
    }
}
