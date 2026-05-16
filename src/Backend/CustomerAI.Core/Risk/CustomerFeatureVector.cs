namespace CustomerAI.Core.Risk
{
    public class CustomerFeatureVector
    {
        public int CustomerId { get; set; }
        public double TotalSpend { get; set; }
        public int MembershipDays { get; set; }
        public int RecencyDays { get; set; }
        public int OrderCount { get; set; }
        public double AverageOrderValue { get; set; }
        public double AverageOrderGapDays { get; set; }
        public double PurchaseFrequency { get; set; }
        public float LastSentimentScore { get; set; }
        public float AverageSentimentScore { get; set; }
        public int InteractionCount { get; set; }
        public int ComplaintCount { get; set; }
        public double SpendLast30Days { get; set; }
        public double SpendLast90Days { get; set; }
        public double SpendDropRate { get; set; }
    }
}
