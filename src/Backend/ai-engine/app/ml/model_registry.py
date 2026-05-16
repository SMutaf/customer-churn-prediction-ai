import json
import os
from datetime import datetime

import joblib
import pandas as pd

from .feature_schema import FEATURE_COLUMNS, feature_schema_payload

APP_DIR = os.path.dirname(os.path.dirname(__file__))
MODEL_PATH = os.path.join(APP_DIR, "churn_model.pkl")
METADATA_PATH = os.path.join(APP_DIR, "model_metadata.json")
FEATURE_SCHEMA_PATH = os.path.join(APP_DIR, "feature_schema.json")
REFERENCE_FEATURES_PATH = os.path.join(APP_DIR, "reference_features.csv")


def model_exists():
    return os.path.exists(MODEL_PATH)


def load_model():
    if not model_exists():
        return None
    return joblib.load(MODEL_PATH)


def load_feature_schema():
    if not os.path.exists(FEATURE_SCHEMA_PATH):
        return feature_schema_payload()
    with open(FEATURE_SCHEMA_PATH, "r", encoding="utf-8") as file:
        return json.load(file)


def load_metadata():
    if not os.path.exists(METADATA_PATH):
        return {"version": "untrained", "feature_schema": [item["name"] for item in feature_schema_payload()["features"]]}
    with open(METADATA_PATH, "r", encoding="utf-8") as file:
        return json.load(file)


def expected_feature_names():
    schema = load_feature_schema()
    features = schema.get("features", [])
    if not features:
        return FEATURE_COLUMNS
    return [item["name"] for item in sorted(features, key=lambda item: item.get("order", 0))]


def load_reference_features(feature_names=None):
    if not os.path.exists(REFERENCE_FEATURES_PATH):
        return None

    reference_features = pd.read_csv(REFERENCE_FEATURES_PATH)
    expected_features = feature_names or expected_feature_names()
    missing_features = [feature for feature in expected_features if feature not in reference_features.columns]
    if missing_features:
        return None

    return reference_features[expected_features]


def save_reference_features(feature_frame, max_rows=5000):
    reference_features = feature_frame[FEATURE_COLUMNS].copy()
    if len(reference_features) > max_rows:
        reference_features = reference_features.sample(n=max_rows, random_state=42)

    reference_features.to_csv(REFERENCE_FEATURES_PATH, index=False)
    return {
        "path": os.path.basename(REFERENCE_FEATURES_PATH),
        "row_count": int(len(reference_features)),
        "format": "csv",
    }


def save_model_artifacts(model, metrics, feature_importance, training_row_count, model_type="RandomForestClassifier", reference_info=None):
    version = datetime.utcnow().strftime("%Y%m%d%H%M%S")
    joblib.dump(model, MODEL_PATH)

    schema = feature_schema_payload()
    metadata = {
        "version": version,
        "trained_at": datetime.utcnow().isoformat() + "Z",
        "model_type": model_type,
        "feature_schema": FEATURE_COLUMNS,
        "accuracy": metrics["accuracy"],
        "precision": metrics["precision"],
        "recall": metrics["recall"],
        "f1_score": metrics["f1_score"],
        "feature_importance": feature_importance,
        "training_row_count": int(training_row_count),
        "reference_dataset": reference_info or {},
    }

    with open(METADATA_PATH, "w", encoding="utf-8") as file:
        json.dump(metadata, file, ensure_ascii=False, indent=2)

    with open(FEATURE_SCHEMA_PATH, "w", encoding="utf-8") as file:
        json.dump(schema, file, ensure_ascii=False, indent=2)

    return metadata
