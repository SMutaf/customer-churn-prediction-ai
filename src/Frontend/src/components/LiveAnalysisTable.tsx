import { ChevronDown, ChevronRight } from "lucide-react";
import { Fragment, useState } from "react";
import type { LivePrediction, RecentPrediction } from "../types";
import { formatScore, parseExplanation, riskLabel } from "../utils";
import { ShapFactorsPanel } from "./ShapFactorsPanel";

interface Props {
  livePredictions: LivePrediction[];
  recentPredictions: RecentPrediction[];
}

export function LiveAnalysisTable({ livePredictions, recentPredictions }: Props) {
  const [openRows, setOpenRows] = useState<Record<string, boolean>>({});
  const rows: LivePrediction[] = livePredictions.length > 0
    ? livePredictions
    : recentPredictions.map((prediction) => ({
      customerId: prediction.customerId,
      churnScore: prediction.churnScore,
      riskLevel: prediction.riskLevel,
      recommendedAction: prediction.recommendedAction,
      mainReason: prediction.mainReason,
    }));

  return (
    <section className="panel table-panel">
      <div className="panel-header">
        <h2>{livePredictions.length > 0 ? "Canlı Analiz Akışı" : "Son Analizler"}</h2>
        <span>{rows.length} kayıt</span>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th></th>
              <th>Müşteri</th>
              <th>Risk</th>
              <th>Skor</th>
              <th>Segment</th>
              <th>Neden</th>
              <th>Aksiyon</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, index) => {
              const key = `${row.customerId}-${row.predictionDate ?? index}`;
              const isOpen = openRows[key] ?? false;
              const explanation = parseExplanation(row.modelExplanationsJson);

              return (
                <Fragment key={key}>
                  <tr key={key}>
                    <td>
                      <button
                        className="row-toggle"
                        onClick={() => setOpenRows((current) => ({ ...current, [key]: !isOpen }))}
                        title="SHAP detayını aç"
                        aria-label="SHAP detayını aç"
                      >
                        {isOpen ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                      </button>
                    </td>
                    <td className="customer-cell">Müşteri #{row.customerId}</td>
                    <td><span className={`risk-badge ${String(row.riskLevel).toLowerCase()}`}>{riskLabel(row.riskLevel)}</span></td>
                    <td>{formatScore(row.churnScore)}</td>
                    <td>{row.segment ?? "-"}</td>
                    <td>{row.mainReason ?? "-"}</td>
                    <td>{row.recommendedAction}</td>
                  </tr>
                  {isOpen && (
                    <tr className="detail-row" key={`${key}-detail`}>
                      <td></td>
                      <td colSpan={6}>
                        <ShapFactorsPanel explanation={explanation} />
                      </td>
                    </tr>
                  )}
                </Fragment>
              );
            })}
          </tbody>
        </table>
      </div>
    </section>
  );
}
