import pandas as pd
import pyodbc
import joblib
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score, classification_report
import datetime
import os

# LocalDB  ayarları
conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\mssqllocaldb;"
    r"Database=CustomerAiDb;"
    r"Trusted_Connection=yes;"
)

def get_data_from_sql():
    print("Veritabanından veriler çekiliyor...")
    try:
        conn = pyodbc.connect(conn_str)
    except Exception as e:
        print("Bağlantı Hatası! Lütfen 'ODBC Driver 17 for SQL Server' yüklü mü kontrol et.")
        print(f"Hata detayı: {e}")
        return None, None, None

    customers = pd.read_sql("SELECT * FROM Customers", conn)
    orders = pd.read_sql("SELECT * FROM Orders", conn)
    interactions = pd.read_sql("SELECT * FROM Interactions", conn)
    
    conn.close()
    return customers, orders, interactions

def prepare_features(customers, orders, interactions):
    print(" Özellik Mühendisliği (Feature Engineering) yapılıyor...")
    
    features = []
    now = datetime.datetime.now()

    for _, customer in customers.iterrows():
        cust_id = customer['Id']
        
        cust_orders = orders[orders['CustomerId'] == cust_id]
        cust_interactions = interactions[interactions['CustomerId'] == cust_id]
        
        total_spend = cust_orders['TotalAmount'].sum() if not cust_orders.empty else 0
        
        mem_date = customer['MembershipDate']
        if isinstance(mem_date, str):
            mem_date = datetime.datetime.strptime(mem_date.split('.')[0], "%Y-%m-%d %H:%M:%S")
            
        membership_days = (now - mem_date).days
        
        if not cust_orders.empty:
            last_order_date = cust_orders['OrderDate'].max()
            if isinstance(last_order_date, str):
                last_order_date = datetime.datetime.strptime(last_order_date.split('.')[0], "%Y-%m-%d %H:%M:%S")
            recency = (now - last_order_date).days
        else:
            recency = 999 
            
        if not cust_interactions.empty:
            last_interaction = cust_interactions.sort_values(by='Date', ascending=False).iloc[0]
            last_sentiment = last_interaction['SentimentScore']
        else:
            last_sentiment = 0 

        #  180 gündür sipariş vermediyse müşteri kayıp.
        is_churn = 1 if recency > 180 else 0
        
        features.append({
            'total_spend': total_spend,
            'membership_days': membership_days,
            'last_interaction_score': last_sentiment,
            'is_churn': is_churn 
        })
        
    return pd.DataFrame(features)

def train():
    cust, ords, inter = get_data_from_sql()
    if cust is None: return

    df = prepare_features(cust, ords, inter)
    print(f"Toplam {len(df)} satır veri işlendi.")
    
    X = df[['total_spend', 'membership_days', 'last_interaction_score']]
    y = df['is_churn']
    
    # %80 Eğitim, %20 Test
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    
    print("Model eğitiliyor (Random Forest)...")
    model = RandomForestClassifier(n_estimators=100, random_state=42)
    model.fit(X_train, y_train)
    
    # test
    y_pred = model.predict(X_test)
    accuracy = accuracy_score(y_test, y_pred)
    print(f"Model Doğruluğu: {accuracy:.2f}")
    
    output_path = os.path.join("app", "churn_model.pkl")
    joblib.dump(model, output_path)
    print(f"Model başarıyla kaydedildi: {output_path}")

if __name__ == "__main__":
    train()