import type { DashboardSummary } from "../types";
import { riskLabel } from "../utils";

interface Props {
  summary: DashboardSummary | null;
}

const order = ["Low", "Medium", "High", "Critical"];

export function RiskDistribution({ summary }: Props) {
  const distribution = summary?.riskDistribution ?? {};
  const total = Object.values(distribution).reduce((sum, value) => sum + value, 0);

  return (
    <section className="panel distribution-panel">
      <div className="panel-header">
        <h2>Risk Dağılımı</h2>
      </div>

      <div className="distribution-list">
        {order.map((level) => {
          const count = distribution[level] ?? 0;
          const width = total > 0 ? Math.max(4, Math.round((count / total) * 100)) : 0;

          return (
            <div className="distribution-row" key={level}>
              <div className="distribution-label">
                <span className={`risk-dot ${level.toLowerCase()}`} />
                <span>{riskLabel(level)}</span>
                <strong>{count}</strong>
              </div>
              <div className="mini-track">
                <div className={`mini-fill ${level.toLowerCase()}`} style={{ width: `${width}%` }} />
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}
