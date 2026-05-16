import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, f1_score, precision_score, recall_score
from sklearn.model_selection import train_test_split

from .behavior_profile_builder import CustomerBehaviorProfileBuilder
from .data_loader import SqlDataLoader
from .explainability_service import ExplainabilityService
from .feature_extractor import FeatureExtractor
from .feature_schema import FEATURE_COLUMNS
from .labeling_engine import LabelingEngine
from .model_registry import save_model_artifacts, save_reference_features


class ModelTrainer:
    def __init__(self, data_loader=None, model_factory=None):
        self.data_loader = data_loader or SqlDataLoader()
        self.profile_builder = CustomerBehaviorProfileBuilder()
        self.feature_extractor = FeatureExtractor()
        self.labeling_engine = LabelingEngine()
        self.explainability = ExplainabilityService()
        self.model_factory = model_factory or self.default_model_factory

    def default_model_factory(self):
        return RandomForestClassifier(n_estimators=150, random_state=42, class_weight="balanced")

    def build_training_dataset(self):
        customers, orders, interactions = self.data_loader.load()
        if customers is None or customers.empty:
            raise RuntimeError("No customer data found for training")

        profiles = self.profile_builder.build_many(customers, orders, interactions)
        features = self.feature_extractor.extract_many(profiles)
        labels = pd.DataFrame(self.labeling_engine.label_dataset(features))
        return pd.concat([features, labels], axis=1)

    def train(self):
        dataset = self.build_training_dataset()
        x = dataset[FEATURE_COLUMNS]
        y = dataset["is_churn"]

        stratify = y if y.nunique() > 1 else None
        x_train, x_test, y_train, y_test = train_test_split(
            x, y, test_size=0.2, random_state=42, stratify=stratify
        )

        model = self.model_factory()
        model.fit(x_train, y_train)
        metrics = self.evaluate(model, x_test, y_test)
        feature_importance = self.explainability.get_feature_importance(model, FEATURE_COLUMNS)
        reference_info = save_reference_features(x)
        metadata = save_model_artifacts(model, metrics, feature_importance, len(dataset), type(model).__name__, reference_info)
        return {"dataset": dataset, "metrics": metrics, "metadata": metadata}

    def evaluate(self, model, x_test, y_test):
        predictions = model.predict(x_test)
        return {
            "accuracy": round(float(accuracy_score(y_test, predictions)), 4),
            "precision": round(float(precision_score(y_test, predictions, zero_division=0)), 4),
            "recall": round(float(recall_score(y_test, predictions, zero_division=0)), 4),
            "f1_score": round(float(f1_score(y_test, predictions, zero_division=0)), 4),
        }
