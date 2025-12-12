from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import uvicorn
import joblib
import pandas as pd
import os

app = FastAPI(title="CustomerAI Engine", version="2.0")

# eğitilmiş modeli yüklüyoruz
model_path = os.path.join(os.path.dirname(__file__), "churn_model.pkl")

try:
    model = joblib.load(model_path)
    print(f"Model başarıyla yüklendi: {model_path}")
except Exception as e:
    print(f"Model yüklenemedi! Hata: {e}")
    model = None

class CustomerData(BaseModel):
    customer_id: int
    sector: str
    membership_days: int
    total_spend: float
    last_interaction_score: float

@app.get("/")
def home():
    status = "Active " if model else "Model Not Found ⚠️"
    return {"message": "CustomerAI Real Engine", "model_status": status}

@app.post("/predict/churn")
def predict_churn(data: CustomerData):
    if not model:
        raise HTTPException(status_code=500, detail="AI Modeli sunucuda bulunamadı!")

    features = pd.DataFrame([{
        'total_spend': data.total_spend,
        'membership_days': data.membership_days,
        'last_interaction_score': data.last_interaction_score
    }])

    try:
        prediction = model.predict(features)[0]
        probability = model.predict_proba(features)[0][1]

        segment = "Yüksek Riskli" if probability > 0.7 else "Orta Riskli" if probability > 0.4 else "Sadık Müşteri"
        
        advice = "Teşekkür Maili Gönder"
        if probability > 0.75:
            advice = "Acil İndirim Tanımla (%20)"
        elif probability > 0.5:
            advice = "Arama Yap ve Sorun Sor"

        return {
            "customer_id": data.customer_id,
            "churn_risk_score": round(probability, 2), 
            "segment": segment,
            "ai_advice": advice
        }

    except Exception as e:
        return {"error": str(e)}

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)