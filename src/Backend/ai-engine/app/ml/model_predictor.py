from .explainability_service import ExplainabilityService
from .feature_extractor import FeatureExtractor
from .model_registry import expected_feature_names, load_metadata, load_model, load_reference_features


class ModelPredictor:
    def __init__(self):
        self.model = load_model()
        self.metadata = load_metadata()
        self.feature_names = self.resolve_feature_names()
        self.feature_extractor = FeatureExtractor()
        self.explainability = ExplainabilityService()

    @property
    def is_ready(self):
        return self.model is not None

    def reload(self):
        self.model = load_model()
        self.metadata = load_metadata()
        self.feature_names = self.resolve_feature_names()

    def resolve_feature_names(self):
        if self.model is not None and hasattr(self.model, "feature_names_in_"):
            return list(self.model.feature_names_in_)
        return expected_feature_names()

    def predict(self, request_dict, include_explanation=True, include_raw_shap=False):
        customer_id = request_dict.get("customer_id", 0)
        if not self.is_ready:
            return {
                "customer_id": customer_id,
                "status": "model_not_trained",
                "message": "Model not trained",
                "churn_probability": 0.0,
                "predicted_class": 0,
                "confidence_score": 0.0,
                "top_feature_impacts": [],
                "model_explanations": {},
                "model_version": "untrained",
                "churn_risk_score": 0.0,
                "segment": "ML Not Trained",
                "ai_advice": "",
                "main_reason": "Model not trained",
        }

        feature_frame = self.feature_extractor.request_to_frame(request_dict, self.feature_names)
        churn_class_index = self.explainability.resolve_churn_class_index(self.model)
        probability = float(self.model.predict_proba(feature_frame)[0][churn_class_index])
        predicted_class = int(probability >= 0.5)
        confidence = probability if predicted_class == 1 else 1 - probability
        explanations = (
            self.explainability.explain_prediction(
                self.model,
                feature_frame,
                self.feature_names,
                prediction_value=probability,
                include_raw_shap=include_raw_shap,
            )
            if include_explanation
            else {"method": "disabled"}
        )
        top_impacts = self.resolve_top_impacts(explanations, feature_frame)

        return {
            "customer_id": customer_id,
            "status": "ok",
            "churn_probability": round(probability, 4),
            "predicted_class": predicted_class,
            "confidence_score": round(float(confidence), 4),
            "top_feature_impacts": top_impacts,
            "model_explanations": explanations,
            "model_version": self.metadata.get("version", "unknown"),
            "churn_risk_score": round(probability, 4),
            "segment": "ML Only",
            "ai_advice": "",
            "main_reason": "",
        }

    def resolve_top_impacts(self, explanations, feature_frame):
        if explanations.get("method") == "SHAP":
            return explanations.get("top_positive_factors", [])[:5]
        return self.explainability.top_feature_impacts(self.model, feature_frame, self.feature_names)

    def feature_importance_analysis(self):
        reference_features = load_reference_features(self.feature_names)
        analysis = self.explainability.feature_importance_analysis(self.model, self.feature_names, reference_features)
        analysis["model_version"] = self.metadata.get("version", "unknown")
        analysis["trained_at"] = self.metadata.get("trained_at", "")
        return analysis

    def pdp_analysis(self, feature):
        reference_features = load_reference_features(self.feature_names)
        analysis = self.explainability.pdp_analysis(self.model, reference_features, feature, self.feature_names)
        analysis["model_version"] = self.metadata.get("version", "unknown")
        return analysis

    def ale_analysis(self, feature):
        reference_features = load_reference_features(self.feature_names)
        analysis = self.explainability.ale_analysis(self.model, reference_features, feature, self.feature_names)
        analysis["model_version"] = self.metadata.get("version", "unknown")
        return analysis
