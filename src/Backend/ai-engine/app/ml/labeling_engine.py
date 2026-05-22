from dataclasses import dataclass


@dataclass
class LabelResult:
    is_churn: int
    label_score: float
    label_reasons: list[str]


class LabelingEngine:
    def label(self, features):
        score = 0.0
        reasons = []
        strong_risk_reasons = []

        if features["recency_days"] > 180:
            score += 30
            reasons.append("recency_high")
            strong_risk_reasons.append("recency_high")
        elif features["recency_days"] > 90:
            score += 18
            reasons.append("recency_medium")

        if features["last_interaction_score"] < 2.5:
            score += 24
            reasons.append("sentiment_low")
            strong_risk_reasons.append("sentiment_low")

        if features["spend_drop_rate"] <= -0.5:
            score += 18
            reasons.append("spend_drop_high")
            strong_risk_reasons.append("spend_drop_high")

        if features["interaction_count"] == 0 and features["membership_days"] > 90:
            score += 12
            reasons.append("engagement_drop")

        if features["complaint_count"] >= 2:
            score += 18
            reasons.append("complaint_risk")
            strong_risk_reasons.append("complaint_risk")
        elif features["complaint_count"] == 1:
            score += 10
            reasons.append("complaint_watch")

        if features["order_count"] <= 1 and features["membership_days"] > 180:
            score += 16
            reasons.append("low_frequency")

        if features["average_order_gap_days"] > 120:
            score += 10
            reasons.append("long_order_gap")

        if features["total_spend"] < 500 and features["membership_days"] > 100:
            score += 12
            reasons.append("low_lifetime_spend")

        if features["order_count"] == 0 and features["membership_days"] > 60:
            score += 18
            reasons.append("dormant_no_order")
            strong_risk_reasons.append("dormant_no_order")

        score = min(score, 100)
        is_churn = score >= 35
        return LabelResult(is_churn=1 if is_churn else 0, label_score=score, label_reasons=reasons)

    def label_dataset(self, feature_frame):
        labels = []
        for _, row in feature_frame.iterrows():
            result = self.label(row)
            labels.append(
                {
                    "is_churn": result.is_churn,
                    "label_score": result.label_score,
                    "label_reasons": "|".join(result.label_reasons),
                }
            )
        return labels
