import datetime
from dataclasses import dataclass

import pandas as pd


@dataclass
class CustomerBehaviorProfile:
    customer_id: int
    membership_days: int
    total_spend: float
    order_count: int
    last_order_date: datetime.datetime | None
    recency_days: int
    average_order_value: float
    average_order_gap_days: float
    last_sentiment_score: float
    average_sentiment_score: float
    interaction_count: int
    complaint_count: int
    spend_last_30_days: float
    spend_last_90_days: float
    previous_90_days_spend: float


def parse_date(value):
    if pd.isna(value):
        return None
    if isinstance(value, datetime.datetime):
        return value
    return pd.to_datetime(value).to_pydatetime()


def spend_in_window(orders, now, days):
    if orders.empty:
        return 0.0
    start = now - datetime.timedelta(days=days)
    mask = orders["OrderDate"].apply(lambda value: (parse_date(value) or datetime.datetime.min) >= start)
    return float(orders[mask]["TotalAmount"].sum())


def spend_between(orders, now, from_days_ago, to_days_ago):
    if orders.empty:
        return 0.0
    start = now - datetime.timedelta(days=from_days_ago)
    end = now - datetime.timedelta(days=to_days_ago)
    mask = orders["OrderDate"].apply(lambda value: start <= (parse_date(value) or datetime.datetime.min) < end)
    return float(orders[mask]["TotalAmount"].sum())


def average_order_gap_days(orders):
    if orders.empty or len(orders) < 2:
        return 0.0
    dates = sorted([parse_date(value) for value in orders["OrderDate"] if parse_date(value) is not None])
    if len(dates) < 2:
        return 0.0
    gaps = [(dates[i] - dates[i - 1]).days for i in range(1, len(dates))]
    return round(float(sum(gaps) / len(gaps)), 2)


class CustomerBehaviorProfileBuilder:
    def build_many(self, customers, orders, interactions):
        now = datetime.datetime.now()
        profiles = []
        for _, customer in customers.iterrows():
            customer_id = customer["Id"]
            customer_orders = orders[orders["CustomerId"] == customer_id]
            customer_interactions = interactions[interactions["CustomerId"] == customer_id]
            profiles.append(self.build(customer, customer_orders, customer_interactions, now))
        return profiles

    def build(self, customer, orders, interactions, now=None):
        now = now or datetime.datetime.now()
        membership_date = parse_date(customer["MembershipDate"]) or now
        order_count = int(len(orders))
        total_spend = float(orders["TotalAmount"].sum()) if not orders.empty else 0.0
        order_dates = [parse_date(value) for value in orders["OrderDate"]] if not orders.empty else []
        last_order_date = max([value for value in order_dates if value is not None], default=None)
        recency_days = max(0, (now - last_order_date).days) if last_order_date else 999

        if interactions.empty:
            last_sentiment = 3.0
            average_sentiment = 3.0
            complaint_count = 0
        else:
            interaction_copy = interactions.copy()
            interaction_copy["ParsedDate"] = interaction_copy["Date"].apply(parse_date)
            interaction_copy = interaction_copy.sort_values(by="ParsedDate", ascending=False)
            sentiment_scores = interaction_copy["SentimentScore"].dropna().astype(float).tolist()
            last_sentiment = sentiment_scores[0] if sentiment_scores else 3.0
            average_sentiment = sum(sentiment_scores) / len(sentiment_scores) if sentiment_scores else 3.0
            complaint_count = int((interaction_copy["Type"] == 3).sum())

        return CustomerBehaviorProfile(
            customer_id=int(customer["Id"]),
            membership_days=max(0, (now - membership_date).days),
            total_spend=total_spend,
            order_count=order_count,
            last_order_date=last_order_date,
            recency_days=recency_days,
            average_order_value=total_spend / order_count if order_count > 0 else 0.0,
            average_order_gap_days=average_order_gap_days(orders),
            last_sentiment_score=float(last_sentiment),
            average_sentiment_score=float(average_sentiment),
            interaction_count=int(len(interactions)),
            complaint_count=complaint_count,
            spend_last_30_days=spend_in_window(orders, now, 30),
            spend_last_90_days=spend_in_window(orders, now, 90),
            previous_90_days_spend=spend_between(orders, now, 180, 90),
        )
