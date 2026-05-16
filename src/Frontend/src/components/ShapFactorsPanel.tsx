import { TrendingDown, TrendingUp } from "lucide-react";
import type { ReactNode } from "react";
import type { ModelExplanation, ShapFactor } from "../types";

interface Props {
  explanation: ModelExplanation | null;
}

function FactorList({ title, icon, factors }: { title: string; icon: ReactNode; factors: ShapFactor[] }) {
  if (factors.length === 0) {
    return (
      <div className="factor-group">
        <h4>{icon}{title}</h4>
        <p className="empty-text">Kayıt yok</p>
      </div>
    );
  }

  return (
    <div className="factor-group">
      <h4>{icon}{title}</h4>
      {factors.map((factor) => (
        <div className="factor-row" key={`${factor.feature}-${factor.shap_value}`}>
          <div>
            <strong>{factor.feature}</strong>
            <span>{factor.explanation}</span>
          </div>
          <code>{factor.shap_value.toFixed(4)}</code>
        </div>
      ))}
    </div>
  );
}

export function ShapFactorsPanel({ explanation }: Props) {
  if (!explanation) {
    return <p className="empty-text">SHAP açıklaması bulunamadı.</p>;
  }

  if (explanation.method === "fallback") {
    return <p className="empty-text">Fallback explanation: {explanation.error}</p>;
  }

  return (
    <div className="shap-panel">
      <div className="shap-meta">
        <span>Method: {explanation.method ?? "-"}</span>
        <span>Base: {explanation.base_value?.toFixed(4) ?? "-"}</span>
        <span>Prediction: {explanation.prediction_value?.toFixed(4) ?? "-"}</span>
      </div>
      <div className="factor-grid">
        <FactorList
          title="Riski artıranlar"
          icon={<TrendingUp size={15} />}
          factors={explanation.top_positive_factors ?? []}
        />
        <FactorList
          title="Riski azaltanlar"
          icon={<TrendingDown size={15} />}
          factors={explanation.top_negative_factors ?? []}
        />
      </div>
    </div>
  );
}
