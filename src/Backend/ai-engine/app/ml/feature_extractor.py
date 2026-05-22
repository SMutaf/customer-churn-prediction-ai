import math

import pandas as pd

from .feature_schema import FEATURE_COLUMNS


def clean_number(value, default=0.0):
    if value is None:
        return default
    if not isinstance(value, (int, float)):
        return default
    if math.isnan(value) or math.isinf(value):
        return default
    return value


class FeatureExtractor:
    def extract(self, profile):
        spend_drop_rate = (
            (profile.spend_last_90_days - profile.previous_90_days_spend) / profile.previous_90_days_spend
            if profile.previous_90_days_spend > 0
            else 0.0
        )

        return {
            "total_spend": float(profile.total_spend),
            "membership_days": int(profile.membership_days),
            "recency_days": int(profile.recency_days),
            "order_count": int(profile.order_count),
            "average_order_value": float(profile.average_order_value),
            "average_order_gap_days": float(profile.average_order_gap_days),
            "purchase_frequency": float(profile.order_count / profile.membership_days) if profile.membership_days > 0 else 0.0,
            "last_interaction_score": float(profile.last_sentiment_score),
            "average_sentiment_score": float(profile.average_sentiment_score),
            "interaction_count": int(profile.interaction_count),
            "complaint_count": int(profile.complaint_count),
            "spend_last_30_days": float(profile.spend_last_30_days),
            "spend_last_90_days": float(profile.spend_last_90_days),
            "spend_drop_rate": float(spend_drop_rate),
            "recency_bucket": (
                3 if profile.recency_days > 180
                else 2 if profile.recency_days > 90
                else 1 if profile.recency_days > 30
                else 0
            ),
            "spend_trend_flag": (
                1 if spend_drop_rate <= -0.40 else 0
            ),
        }

    def extract_many(self, profiles):
        rows = [self.extract(profile) for profile in profiles]
        return pd.DataFrame(rows, columns=FEATURE_COLUMNS)

    def request_to_frame(self, request_dict, expected_features=None):
        expected_features = expected_features or FEATURE_COLUMNS
        row = {}
        for feature in expected_features:
            default = 3.0 if "sentiment" in feature or feature == "last_interaction_score" else 0.0
            if feature == "recency_days":
                default = 999
            if feature == "recency_bucket":
                recency_days = clean_number(request_dict.get("recency_days"), 999)
                row[feature] = (
                    3 if recency_days > 180
                    else 2 if recency_days > 90
                    else 1 if recency_days > 30
                    else 0
                )
            elif feature == "spend_trend_flag":
                spend_drop_rate = clean_number(request_dict.get("spend_drop_rate"), 0.0)
                row[feature] = 1 if spend_drop_rate <= -0.40 else 0
            else:
                row[feature] = clean_number(request_dict.get(feature), default)
        return pd.DataFrame([row], columns=expected_features)
