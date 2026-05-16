using CustomerAI.Core.Enums;
using CustomerAI.Core.Risk;
using CustomerAI.Services.Interfaces;

namespace CustomerAI.Services.Concrete
{
    public class CoreRiskEngine : ICoreRiskEngine
    {
        private const double HighRecencyImpact = 30;
        private const double MediumRecencyImpact = 18;
        private const double LowFrequencyImpact = 16;
        private const double MonetaryDropImpact = 18;
        private const double LowMonetaryImpact = 12;
        private const double SentimentImpact = 24;
        private const double ComplaintImpact = 18;
        private const double EngagementImpact = 12;

        public CustomerRiskProfile Evaluate(CustomerFeatureVector featureVector)
        {
            var rules = new List<RiskRuleResult>();

            AddRecencyRules(featureVector, rules);
            AddFrequencyRules(featureVector, rules);
            AddMonetaryRules(featureVector, rules);
            AddSentimentRules(featureVector, rules);
            AddEngagementRules(featureVector, rules);

            var coreScore = Math.Clamp(rules.Sum(r => r.ScoreImpact), 0, 100);

            return new CustomerRiskProfile
            {
                CustomerId = featureVector.CustomerId,
                CoreRiskScore = Math.Round(coreScore, 2),
                TriggeredRules = rules,
                MainReason = rules.OrderByDescending(r => r.Severity).ThenByDescending(r => r.ScoreImpact).FirstOrDefault()?.Reason
                    ?? "Belirgin bir iş riski bulunmadı."
            };
        }

        private static void AddRecencyRules(CustomerFeatureVector features, List<RiskRuleResult> rules)
        {
            if (features.RecencyDays > 180)
            {
                rules.Add(CreateRule(RiskCategory.Recency, "RECENCY_HIGH", "180 günden uzun süredir sipariş yok.", HighRecencyImpact, 3));
            }
            else if (features.RecencyDays > 90)
            {
                rules.Add(CreateRule(RiskCategory.Recency, "RECENCY_MEDIUM", "90 günden uzun süredir sipariş yok.", MediumRecencyImpact, 2));
            }
        }

        private static void AddFrequencyRules(CustomerFeatureVector features, List<RiskRuleResult> rules)
        {
            if (features.OrderCount <= 1 && features.MembershipDays > 180)
            {
                rules.Add(CreateRule(RiskCategory.Frequency, "LOW_ORDER_COUNT_MATURE_CUSTOMER", "Uzun üyelik süresine rağmen sipariş sayısı düşük.", LowFrequencyImpact, 2));
            }

            if (features.AverageOrderGapDays > 120)
            {
                rules.Add(CreateRule(RiskCategory.Frequency, "LONG_ORDER_GAP", "Siparişler arasındaki ortalama süre yüksek.", 10, 2));
            }
        }

        private static void AddMonetaryRules(CustomerFeatureVector features, List<RiskRuleResult> rules)
        {
            if (features.SpendDropRate <= -0.5)
            {
                rules.Add(CreateRule(RiskCategory.Monetary, "SPEND_DROP_HIGH", "Son 90 günlük harcama önceki döneme göre sert düşmüş.", MonetaryDropImpact, 3));
            }

            if (features.TotalSpend < 500 && features.MembershipDays > 100)
            {
                rules.Add(CreateRule(RiskCategory.Monetary, "LOW_LIFETIME_SPEND", "Üyelik süresine göre toplam harcama düşük.", LowMonetaryImpact, 2));
            }
        }

        private static void AddSentimentRules(CustomerFeatureVector features, List<RiskRuleResult> rules)
        {
            if (features.LastSentimentScore < 2.5)
            {
                rules.Add(CreateRule(RiskCategory.Sentiment, "LOW_LAST_SENTIMENT", "Son müşteri etkileşimi memnuniyetsiz görünüyor.", SentimentImpact, 3));
            }

            if (features.ComplaintCount >= 2)
            {
                rules.Add(CreateRule(RiskCategory.Sentiment, "MULTIPLE_COMPLAINTS", "Birden fazla şikayet kaydı var.", ComplaintImpact, 3));
            }
        }

        private static void AddEngagementRules(CustomerFeatureVector features, List<RiskRuleResult> rules)
        {
            if (features.InteractionCount == 0 && features.MembershipDays > 90)
            {
                rules.Add(CreateRule(RiskCategory.Engagement, "NO_ENGAGEMENT", "Müşteriyle kaydedilmiş etkileşim yok.", EngagementImpact, 2));
            }

            if (features.OrderCount == 0 && features.MembershipDays > 60)
            {
                rules.Add(CreateRule(RiskCategory.Engagement, "DORMANT_NO_ORDER", "Üyelik sonrası hiç sipariş oluşmamış.", 18, 3));
            }
        }

        private static RiskRuleResult CreateRule(RiskCategory category, string code, string reason, double impact, int severity)
        {
            return new RiskRuleResult
            {
                Category = category,
                RuleCode = code,
                Reason = reason,
                ScoreImpact = impact,
                Severity = severity
            };
        }
    }
}
