using CustomerAI.Core.DTOs;
using CustomerAI.Core.Enums;
using CustomerAI.Core.Risk;
using CustomerAI.Services.Interfaces;

namespace CustomerAI.Services.Concrete
{
    public class FinalRiskDecisionService : IFinalRiskDecisionService
    {
        private const double CoreRiskWeight = 0.55;
        private const double MlRiskWeight = 0.45;

        public CustomerRiskProfile BuildFinalProfile(CustomerRiskProfile coreProfile, AiResponseDto mlResponse)
        {
            var mlProbability = ResolveMlProbability(mlResponse);
            var mlScore = mlProbability * 100;
            var finalScore = Math.Clamp(coreProfile.CoreRiskScore * CoreRiskWeight + mlScore * MlRiskWeight, 0, 100);

            coreProfile.MlChurnProbability = Math.Round(mlProbability, 4);
            coreProfile.FinalRiskScore = Math.Round(finalScore, 2);
            coreProfile.RiskLevel = ResolveRiskLevel(finalScore);
            coreProfile.RecommendedAction = BuildRecommendedAction(coreProfile);
            coreProfile.CreatedAt = DateTime.Now;

            return coreProfile;
        }

        private static double ResolveMlProbability(AiResponseDto response)
        {
            var probability = response.churn_probability > 0
                ? response.churn_probability
                : response.churn_risk_score;

            return Math.Clamp(probability, 0, 1);
        }

        private static RiskLevel ResolveRiskLevel(double finalScore)
        {
            if (finalScore >= 85) return RiskLevel.Critical;
            if (finalScore >= 65) return RiskLevel.High;
            if (finalScore >= 40) return RiskLevel.Medium;
            return RiskLevel.Low;
        }

        private static string BuildRecommendedAction(CustomerRiskProfile profile)
        {
            return profile.RiskLevel switch
            {
                RiskLevel.Critical => "Acil yönetici teması, telafi teklifi ve kişisel geri kazanım planı.",
                RiskLevel.High => "Müşteri başarı ekibi doğrudan temas kurmalı ve özel teklif sunmalı.",
                RiskLevel.Medium => "Kampanya, memnuniyet kontrolü ve etkileşim artırıcı takip önerilir.",
                _ => "Standart müşteri iletişimi ve düzenli takip yeterli."
            };
        }
    }
}
