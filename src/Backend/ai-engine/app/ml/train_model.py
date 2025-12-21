import pandas as pd
import pyodbc
import joblib
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score
import datetime
import os

# Veritabanı Bağlantısı
conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\mssqllocaldb;"
    r"Database=CustomerAiDb;"
    r"Trusted_Connection=yes;"
)

def get_data_from_sql():
    print("Veritabanına bağlanılıyor...")
    try:
        conn = pyodbc.connect(conn_str)
        customers = pd.read_sql("SELECT * FROM Customers", conn)
        orders = pd.read_sql("SELECT * FROM Orders", conn)
        interactions = pd.read_sql("SELECT * FROM Interactions", conn)
        conn.close()
        return customers, orders, interactions
    except Exception as e:
        print("Bağlantı Hatası! Veritabanı boş olabilir veya Driver eksik.")
        print(f"Hata: {e}")
        return None, None, None

def prepare_features(customers, orders, interactions):
    print("🛠️ Veriler işleniyor ve etiketleniyor...")
    
    features = []
    now = datetime.datetime.now()

    for _, customer in customers.iterrows():
        cust_id = customer['Id']
        
        # Müşteriye ait verileri süz
        cust_orders = orders[orders['CustomerId'] == cust_id]
        cust_interactions = interactions[interactions['CustomerId'] == cust_id]
        
        # Toplam Harcama
        total_spend = cust_orders['TotalAmount'].sum() if not cust_orders.empty else 0
        
        # Üyelik Günü
        mem_date = customer['MembershipDate']
        if isinstance(mem_date, str):
            mem_date = datetime.datetime.strptime(mem_date.split('.')[0], "%Y-%m-%d %H:%M:%S")
        membership_days = (now - mem_date).days
        
        # Son Sipariş 
        if not cust_orders.empty:
            last_order_date = cust_orders['OrderDate'].max()
            if isinstance(last_order_date, str):
                last_order_date = datetime.datetime.strptime(last_order_date.split('.')[0], "%Y-%m-%d %H:%M:%S")
            recency = (now - last_order_date).days
        else:
            recency = 999 
            
        # Son Puan 
        if not cust_interactions.empty:
            # En son etkileşimi al
            last_interaction = cust_interactions.sort_values(by='Date', ascending=False).iloc[0]
            last_sentiment = last_interaction['SentimentScore'] if last_interaction['SentimentScore'] else 3.0
        else:
            last_sentiment = 3.0 # Nötr

        # "KİM KAYIP MÜŞTERİ?"
        is_churn = 0
        
        # Kural 1: Çok uzun süredir gelmiyor (> 180 gün)
        if recency > 180:
            is_churn = 1
            
        # Kural 2: Çok Düşük Puan (< 2.5) -> Sinirli Müşteri Potansiyel Kayıptır!
        if last_sentiment < 2.5:
            is_churn = 1
            
        # Kural 3: Çok az harcamış ve yeni değil (< 500 TL ve > 100 gün)
        if total_spend < 500 and membership_days > 100:
            is_churn = 1

        features.append({
            'total_spend': float(total_spend),
            'membership_days': int(membership_days),
            'last_interaction_score': float(last_sentiment),
            'is_churn': is_churn 
        })
        
    return pd.DataFrame(features)

def train():
    # 1. Veriyi Çek
    cust, ords, inter = get_data_from_sql()
    if cust is None or cust.empty:
        print("⚠️ Veritabanında veri yok! Önce Seed işlemini yapmalısın.")
        return

    # 2. Hazırla
    df = prepare_features(cust, ords, inter)
    print(f"📊 Toplam {len(df)} satır veri eğitim için hazır.")
    
    X = df[['total_spend', 'membership_days', 'last_interaction_score']]
    y = df['is_churn']
    
    # 3. Eğit
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    
    print("🧠 Model eğitiliyor (Random Forest)...")
    model = RandomForestClassifier(n_estimators=100, random_state=42)
    model.fit(X_train, y_train)
    
    # 4. Test Et
    y_pred = model.predict(X_test)
    accuracy = accuracy_score(y_test, y_pred)
    print(f"🎯 Model Doğruluğu: %{accuracy*100:.2f}")
    
    # 5. Kaydet 
    # Dosya yolunu 'app' klasörünün içinde, main.py'nin yanında.
    current_dir = os.path.dirname(os.path.abspath(__file__)) # ml klasörü
    parent_dir = os.path.dirname(current_dir) # app klasörü
    output_path = os.path.join(parent_dir, "churn_model.pkl")
    
    joblib.dump(model, output_path)
    print(f"Model başarıyla kaydedildi: {output_path}")

if __name__ == "__main__":
    train()