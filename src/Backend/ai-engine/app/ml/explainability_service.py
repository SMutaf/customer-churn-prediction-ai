import numpy as np
import pandas as pd
from sklearn.inspection import partial_dependence

try:
    import shap
except Exception as error:
    shap = None
    SHAP_IMPORT_ERROR = str(error)
else:
    SHAP_IMPORT_ERROR = ""


class ExplainabilityService:
    def get_feature_importance(self, model, feature_names):
        if model is None or not hasattr(model, "feature_importances_"):
            return {feature: 0.0 for feature in feature_names}
        return {
            feature: round(float(importance), 6)
            for feature, importance in zip(feature_names, getattr(model, "feature_importances_", []))
        }

    def get_global_feature_importance(self, model, feature_names):
        importances = self.get_feature_importance(model, feature_names)
        return [
            {
                "feature": feature,
                "importance": importance,
                "rank": rank,
            }
            for rank, (feature, importance) in enumerate(
                sorted(importances.items(), key=lambda item: item[1], reverse=True),
                start=1,
            )
        ]

    def explain_prediction(self, model, feature_frame, feature_names, prediction_value=None, include_raw_shap=False, top_n=5):
        if shap is None:
            return self.fallback_explanation(model, feature_frame, feature_names, f"SHAP import failed: {SHAP_IMPORT_ERROR}")

        try:
            explainer = shap.TreeExplainer(model)
            raw_values = explainer.shap_values(feature_frame)
            shap_values = self.resolve_churn_class_values(model, raw_values)
            base_value = self.resolve_churn_base_value(model, explainer.expected_value)
            feature_values = feature_frame.iloc[0].to_dict()
            local_values = np.asarray(shap_values)[0]

            explanation = {
                "method": "SHAP",
                "base_value": round(float(base_value), 6),
                "prediction_value": round(float(prediction_value), 6) if prediction_value is not None else None,
                "top_positive_factors": self.get_top_positive_risk_factors(local_values, feature_values, feature_names, top_n),
                "top_negative_factors": self.get_top_negative_risk_factors(local_values, feature_values, feature_names, top_n),
            }

            if include_raw_shap:
                explanation["raw_shap_values"] = {
                    feature: round(float(value), 6)
                    for feature, value in zip(feature_names, local_values)
                }

            return explanation
        except Exception as error:
            return self.fallback_explanation(model, feature_frame, feature_names, str(error))

    def get_top_positive_risk_factors(self, shap_values, feature_values, feature_names, limit=5):
        factors = [
            self.create_factor(feature, feature_values.get(feature), shap_value, "increases_risk")
            for feature, shap_value in zip(feature_names, shap_values)
            if float(shap_value) > 0
        ]
        return sorted(factors, key=lambda item: item["shap_value"], reverse=True)[:limit]

    def get_top_negative_risk_factors(self, shap_values, feature_values, feature_names, limit=5):
        factors = [
            self.create_factor(feature, feature_values.get(feature), shap_value, "decreases_risk")
            for feature, shap_value in zip(feature_names, shap_values)
            if float(shap_value) < 0
        ]
        return sorted(factors, key=lambda item: item["shap_value"])[:limit]

    def top_feature_impacts(self, model, feature_frame, feature_names, limit=5):
        importances = self.get_feature_importance(model, feature_names)
        row = feature_frame.iloc[0].to_dict()
        impacts = [
            {
                "feature": feature,
                "value": float(row.get(feature, 0.0)),
                "impact": round(float(importances.get(feature, 0.0)), 6),
            }
            for feature in feature_names
        ]
        return sorted(impacts, key=lambda item: abs(item["impact"]), reverse=True)[:limit]

    def feature_importance_analysis(self, model, feature_names, reference_features=None):
        built_in = self.get_global_feature_importance(model, feature_names)
        response = {"built_in_feature_importance": built_in}

        if shap is None or reference_features is None or reference_features.empty:
            response["mean_abs_shap_importance"] = []
            return response

        try:
            sample = reference_features[feature_names].head(500)
            explainer = shap.TreeExplainer(model)
            raw_values = explainer.shap_values(sample)
            shap_values = self.resolve_churn_class_values(model, raw_values)
            mean_abs_values = np.abs(np.asarray(shap_values)).mean(axis=0)
            response["mean_abs_shap_importance"] = [
                {
                    "feature": feature,
                    "importance": round(float(importance), 6),
                    "rank": rank,
                }
                for rank, (feature, importance) in enumerate(
                    sorted(zip(feature_names, mean_abs_values), key=lambda item: item[1], reverse=True),
                    start=1,
                )
            ]
        except Exception as error:
            response["mean_abs_shap_importance"] = []
            response["shap_error"] = str(error)

        return response

    def pdp_analysis(self, model, reference_features, feature, feature_names):
        self.validate_feature(feature, feature_names)
        if reference_features is None or reference_features.empty:
            raise ValueError("Reference dataset not available for PDP analysis")

        result = partial_dependence(model, reference_features[feature_names], [feature], kind="average")
        grid_values = result.get("grid_values", result.get("values"))[0]
        average_predictions = result["average"][0]

        return {
            "feature": feature,
            "method": "PDP",
            "grid_values": [round(float(value), 6) for value in grid_values],
            "average_predictions": [round(float(value), 6) for value in average_predictions],
        }

    def ale_analysis(self, model, reference_features, feature, feature_names, bins=10):
        self.validate_feature(feature, feature_names)
        if reference_features is None or reference_features.empty:
            raise ValueError("Reference dataset not available for ALE analysis")

        data = reference_features[feature_names].copy()
        feature_values = data[feature].to_numpy(dtype=float)
        bin_edges = np.unique(np.quantile(feature_values, np.linspace(0, 1, bins + 1)))
        if len(bin_edges) < 3:
            raise ValueError("Not enough unique feature values for ALE analysis")

        effects = []
        for lower, upper in zip(bin_edges[:-1], bin_edges[1:]):
            mask = (feature_values >= lower) & (feature_values <= upper)
            if not mask.any():
                effects.append(0.0)
                continue

            lower_frame = data.loc[mask].copy()
            upper_frame = data.loc[mask].copy()
            lower_frame[feature] = lower
            upper_frame[feature] = upper

            lower_predictions = model.predict_proba(lower_frame[feature_names])[:, self.resolve_churn_class_index(model)]
            upper_predictions = model.predict_proba(upper_frame[feature_names])[:, self.resolve_churn_class_index(model)]
            effects.append(float(np.mean(upper_predictions - lower_predictions)))

        ale_values = np.cumsum(effects)
        ale_values = ale_values - np.mean(ale_values)

        return {
            "feature": feature,
            "method": "ALE",
            "bin_edges": [round(float(value), 6) for value in bin_edges],
            "ale_values": [round(float(value), 6) for value in ale_values],
        }

    def fallback_explanation(self, model, feature_frame, feature_names, error):
        return {
            "method": "fallback",
            "error": str(error)[:300],
            "top_feature_impacts": self.top_feature_impacts(model, feature_frame, feature_names),
        }

    def resolve_churn_class_index(self, model):
        classes = list(getattr(model, "classes_", []))
        if 1 in classes:
            return classes.index(1)
        if len(classes) > 1:
            return 1
        return 0

    def resolve_churn_class_values(self, model, raw_values):
        class_index = self.resolve_churn_class_index(model)

        if isinstance(raw_values, list):
            return np.asarray(raw_values[min(class_index, len(raw_values) - 1)])

        values = np.asarray(raw_values)
        if values.ndim == 3:
            if values.shape[2] > class_index:
                return values[:, :, class_index]
            if values.shape[0] > class_index:
                return values[class_index, :, :]
        return values

    def resolve_churn_base_value(self, model, expected_value):
        class_index = self.resolve_churn_class_index(model)
        values = np.asarray(expected_value)
        if values.ndim == 0:
            return float(values)
        return float(values[min(class_index, len(values) - 1)])

    def create_factor(self, feature, value, shap_value, direction):
        return {
            "feature": feature,
            "value": round(float(value), 6),
            "shap_value": round(float(shap_value), 6),
            "impact_direction": direction,
            "explanation": self.explain_factor(feature, value, direction),
        }

    def explain_factor(self, feature, value, direction):
        risk_direction = "artiriyor" if direction == "increases_risk" else "azaltiyor"
        templates = {
            "recency_days": f"recency_days degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
            "last_interaction_score": f"last_interaction_score degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
            "average_sentiment_score": f"average_sentiment_score degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
            "complaint_count": f"complaint_count degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
            "spend_drop_rate": f"spend_drop_rate degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
            "order_count": f"order_count degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
            "total_spend": f"total_spend degeri {value:.2f} oldugu icin churn riskini {risk_direction}.",
        }
        return templates.get(feature, f"{feature} degeri {value:.2f} oldugu icin churn riskini {risk_direction}.")

    def validate_feature(self, feature, feature_names):
        if feature not in feature_names:
            raise ValueError(f"Invalid feature: {feature}")
