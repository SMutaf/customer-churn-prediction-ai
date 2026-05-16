import { Activity, AlertTriangle, Radio, Users } from "lucide-react";
import type { AnalysisProgress, DashboardSummary } from "../types";
import { formatPercent } from "../utils";

interface Props {
  summary: DashboardSummary | null;
  progress: AnalysisProgress;
}

export function SummaryCards({ summary, progress }: Props) {
  const cards = [
    {
      label: "Toplam Müşteri",
      value: summary?.totalCustomers ?? 0,
      icon: Users,
      tone: "teal",
    },
    {
      label: "Yüksek Risk",
      value: summary?.highRiskCustomers ?? 0,
      icon: AlertTriangle,
      tone: "coral",
    },
    {
      label: "Ortalama Skor",
      value: formatPercent(summary?.averageChurnScore ?? 0),
      icon: Activity,
      tone: "amber",
    },
    {
      label: "Canlı Bağlantı",
      value: progress.connectionStatus === "connected" ? "Bağlı" : "Bekliyor",
      icon: Radio,
      tone: progress.connectionStatus === "connected" ? "green" : "muted",
    },
  ];

  return (
    <section className="summary-grid">
      {cards.map((card) => {
        const Icon = card.icon;
        return (
          <article className={`summary-card ${card.tone}`} key={card.label}>
            <div>
              <span>{card.label}</span>
              <strong>{card.value}</strong>
            </div>
            <Icon size={24} strokeWidth={2.2} />
          </article>
        );
      })}
    </section>
  );
}
