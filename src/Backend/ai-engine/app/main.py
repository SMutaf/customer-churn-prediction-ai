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

def explain_reason(data: CustomerData, score: float):
    """Müşterinin neden riskli olduğunu analiz eder."""
    if score < 0.45:
        return "Belirgin bir risk faktörü yok."
    
    reasons = []
    
    # 1. duygu faktörü
    if data.last_interaction_score < 3.0:
        reasons.append("Düşük Memnuniyet (Puan < 3)")

    # 2. harcama faktörü
    if data.total_spend < 1500:
        reasons.append("Düşük Harcama Hacmi")

    # 3. yeni üye
    if data.membership_days < 60:
        reasons.append("Yeni Üye Kırılganlığı")

    if reasons:
        return f"Tespit: {reasons[0]}" # İlk bulduğu sebebi döndür
    else:
        return "Sektörel veya Genel Eğilim"

def generate_smart_advice(score: float, reason: str):
    """Skora ve sebebe göre nokta atışı tavsiye verir."""
    if score > 0.7:
        if "Memnuniyet" in reason:
            return "ACİL: Müşteri Temsilcisi Aramalı (Şikayet Çözümü)"
        elif "Harcama" in reason:
            return "Özel İndirim Kuponu Tanımla (%20)"
        else:
            return "VIP İletişime Geçilmeli (Risk Çok Yüksek)"
            
    elif score > 0.45:
        if "Yeni Üye" in reason:
            return "Hoşgeldin Anketi ve Bilgilendirme Maili At"
        else:
            return "E-Bülten ve Kampanya Maili Gönder"
    
    else:
        return "Teşekkür Maili Gönder (Sadık Müşteri)"

@app.get("/")
def home():
    status = "Active " if model else "Model Not Found!"
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
        probability = model.predict_proba(features)[0][1]

        reason_text = explain_reason(data, probability)
        advice_text = generate_smart_advice(probability, reason_text)

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
            "ai_advice": advice_text,
            "main_reason": reason_text
        }

    except Exception as e:
        return {"error": str(e)}

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)