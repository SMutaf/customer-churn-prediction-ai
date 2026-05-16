# CustomerAI Churn Prediction

## Developer Workflow

1. Apply database migrations from `src/Backend/CustomerAI.API`.
2. Start the .NET API and call `POST /api/Seed/generate-fake-data` to create demo customer, order, and interaction data.
3. Train the Python model:

   ```powershell
   cd src/Backend/ai-engine/app
   python train_model.py
   ```

   Alternative:

   ```powershell
   cd src/Backend/ai-engine/app
   python -m ml.train
   ```

4. Verify that these files were created in `src/Backend/ai-engine/app`:
   - `churn_model.pkl`
   - `model_metadata.json`
   - `feature_schema.json`
5. Start the Python FastAPI service:

   ```powershell
   cd src/Backend/ai-engine/app
   python main.py
   ```

6. Start the .NET API from `src/Backend/CustomerAI.API`.
7. Open `src/Frontend/dashboard.html`, run analysis from the dashboard, and verify dashboard predictions.

## ML Pipeline

The training pipeline is organized as:

Raw Customer Data -> CustomerBehaviorProfile generation -> Feature Extraction -> Training Dataset generation -> Label Generation -> Model Training -> Model Evaluation -> Model Export.

Runtime risk decisions are owned by the .NET services. The Python service only returns churn probability, predicted class, confidence, feature impact placeholders, explanation payload, and model version.

## Model Artifacts

`model_metadata.json` stores version, trained time, model type, feature schema, metrics, feature importance, and training row count.

`feature_schema.json` stores the model feature order used by both training and prediction.
