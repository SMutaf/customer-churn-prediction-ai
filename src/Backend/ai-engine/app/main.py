from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import uvicorn
import joblib
import pandas as pd
import os
import traceback
import math

app = FastAPI(title="CustomerAI Engine", version="2.0")

# Model yükleme
model_path = os.path.join(os.path.dirname(__file__), "churn_model.pkl")

try:
    model = joblib.load(model_path)
    print(f"Model başarıyla yüklendi: {model_path}")
except Exception as e:
    print(f"Model yüklenemedi! Hata: {e}")
    model = None


# DTO ayarlama
class CustomerData(BaseModel):
    customer_id: int
    sector: str
    membership_days: int
    total_spend: float
    last_interaction_score: float | None = 0.0



# Reason Engine
def explain_reason(data: CustomerData, score: float) -> str:
    if score < 0.45:
        return "Belirgin bir risk faktörü yok."

    if data.total_spend < 1000 and data.last_interaction_score < 2.0:
        return "Düşük Harcama + Düşük Memnuniyet (Kritik Kombinasyon)"

    if data.last_interaction_score < 1.5:
        return "Çok Düşük Memnuniyet - Şikayet Seviyesi"

    if data.last_interaction_score < 3.0:
        return "Düşük Memnuniyet"

    if data.total_spend < 500:
        return "Çok Düşük Harcama"

    if data.total_spend < 1500:
        return "Düşük Harcama"

    if data.membership_days < 60:
        return "Yeni Üye Kırılganlığı"

    if data.membership_days > 365 and data.total_spend < 2000:
        return "Uzun Süreli Pasif Müşteri"

    return "Genel Davranışsal Risk"


# ai önerisi
def generate_smart_advice(score: float, reason: str, data: CustomerData) -> str:
    if score > 0.7:
        if "Şikayet" in reason:
            return "ACİL: Yönetici Araması + Telafi Paketi"
        if "Kritik" in reason:
            return "ACİL: VIP Destek + %30 İndirim"
        return "Yüksek Risk: Direkt Temas Kurulmalı"

    if score > 0.45:
        if "Yeni Üye" in reason:
            return "Hoşgeldin Kampanyası"
        if "Pasif" in reason:
            return "Sadakat Programı Daveti"
        return "Kampanya ve E-Bülten Gönderimi"

    return "Standart Müşteri İletişimi"


# API
@app.post("/predict/churn")
def predict_churn(data: CustomerData):
    if not model:
        raise HTTPException(status_code=500, detail="AI modeli yüklenemedi")

    try:
        #  NaN / None / Inf TEMİZLEME
        last_score = data.last_interaction_score

        if last_score is None or not isinstance(last_score, (int, float)) or math.isnan(last_score):
            last_score = 0.0

        features = pd.DataFrame([{
            "total_spend": float(data.total_spend),
            "membership_days": int(data.membership_days),
            "last_interaction_score": float(last_score)
        }])

        print("FEATURES >>>")
        print(features)
        print(features.dtypes)

        probability = model.predict_proba(features)[0][1]

        reason = explain_reason(data, probability)
        advice = generate_smart_advice(probability, reason, data)

        return {
            "customer_id": data.customer_id,
            "churn_risk_score": round(probability, 2),
            "segment": (
                "Yüksek Riskli" if probability > 0.7
                else "Orta Riskli" if probability > 0.4
                else "Sadık Müşteri"
            ),
            "ai_advice": advice,
            "main_reason": reason
        }

    except Exception as e:
        print("PYTHON EXCEPTION")
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/")
def health():
    return {
        "status": "OK",
        "model_loaded": model is not None
    }


if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)
