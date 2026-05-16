FEATURE_COLUMNS = [
    "total_spend",
    "membership_days",
    "recency_days",
    "order_count",
    "average_order_value",
    "average_order_gap_days",
    "purchase_frequency",
    "last_interaction_score",
    "average_sentiment_score",
    "interaction_count",
    "complaint_count",
    "spend_last_30_days",
    "spend_last_90_days",
    "spend_drop_rate",
]


def feature_schema_payload():
    return {
        "version": "1.0",
        "features": [{"name": name, "order": index} for index, name in enumerate(FEATURE_COLUMNS)],
    }
