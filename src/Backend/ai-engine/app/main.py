from fastapi import FastAPI, HTTPException, Query
from pydantic import BaseModel
import uvicorn

from ml.feature_schema import FEATURE_COLUMNS
from ml.model_predictor import ModelPredictor

app = FastAPI(title="CustomerAI Engine", version="4.0")
predictor = ModelPredictor()


class CustomerData(BaseModel):
    customer_id: int
    sector: str | None = None
    total_spend: float = 0.0
    membership_days: int = 0
    recency_days: int = 999
    order_count: int = 0
    average_order_value: float = 0.0
    average_order_gap_days: float = 0.0
    purchase_frequency: float = 0.0
    last_interaction_score: float | None = 3.0
    average_sentiment_score: float | None = 3.0
    interaction_count: int = 0
    complaint_count: int = 0
    spend_last_30_days: float = 0.0
    spend_last_90_days: float = 0.0
    spend_drop_rate: float = 0.0


@app.post("/predict/churn")
def predict_churn(
    data: CustomerData,
    include_explanation: bool = Query(default=True),
    include_raw_shap: bool = Query(default=False),
):
    return predictor.predict(
        data.model_dump(),
        include_explanation=include_explanation,
        include_raw_shap=include_raw_shap,
    )


@app.post("/model/reload")
def reload_model():
    predictor.reload()
    return {"status": "ok", "model_loaded": predictor.is_ready}


@app.get("/")
def health():
    return {
        "status": "OK",
        "model_loaded": predictor.is_ready,
        "model_version": predictor.metadata.get("version", "untrained"),
        "features": predictor.feature_names if predictor.is_ready else FEATURE_COLUMNS,
    }


@app.get("/model/analysis/feature-importance")
def feature_importance_analysis():
    if not predictor.is_ready:
        raise HTTPException(status_code=503, detail="Model not trained")
    return predictor.feature_importance_analysis()


@app.get("/model/analysis/pdp")
def pdp_analysis(feature: str):
    if not predictor.is_ready:
        raise HTTPException(status_code=503, detail="Model not trained")
    try:
        return predictor.pdp_analysis(feature)
    except ValueError as error:
        status_code = 400 if str(error).startswith("Invalid feature") else 503
        raise HTTPException(status_code=status_code, detail=str(error))


@app.get("/model/analysis/ale")
def ale_analysis(feature: str):
    if not predictor.is_ready:
        raise HTTPException(status_code=503, detail="Model not trained")
    try:
        return predictor.ale_analysis(feature)
    except ValueError as error:
        status_code = 400 if str(error).startswith("Invalid feature") else 503
        raise HTTPException(status_code=status_code, detail=str(error))


if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)
