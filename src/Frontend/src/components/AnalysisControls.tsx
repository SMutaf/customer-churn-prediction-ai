import { Database, Download, Play, RefreshCw } from "lucide-react";
import type { AnalysisProgress } from "../types";

interface Props {
  progress: AnalysisProgress;
  onAnalyze: () => void;
  onSeed: () => void;
  onRefresh: () => void;
  onExport: () => void;
}

export function AnalysisControls({ progress, onAnalyze, onSeed, onRefresh, onExport }: Props) {
  const percentage = progress.totalCustomers > 0
    ? Math.round((progress.analyzedCustomers / progress.totalCustomers) * 100)
    : 0;

  return (
    <section className="control-band">
      <div className="control-copy">
        <span className="eyebrow">Canlı analiz</span>
        <h1>CustomerAI Risk İzleme</h1>
        <p>{progress.lastMessage ?? "Analiz başlatıldığında müşteri sonuçları anlık olarak akacak."}</p>
      </div>

      <div className="control-actions">
        <button className="primary-button" onClick={onAnalyze} disabled={progress.isRunning}>
          {progress.isRunning ? <RefreshCw className="spin" size={18} /> : <Play size={18} />}
          {progress.isRunning ? "Analiz sürüyor" : "Analizi başlat"}
        </button>
        <button className="icon-button" onClick={onRefresh} title="Yenile" aria-label="Yenile">
          <RefreshCw size={18} />
        </button>
        <button className="icon-button" onClick={onSeed} title="Seed verisi üret" aria-label="Seed verisi üret">
          <Database size={18} />
        </button>
        <button className="icon-button" onClick={onExport} title="Risk raporunu indir" aria-label="Risk raporunu indir">
          <Download size={18} />
        </button>
      </div>

      <div className="progress-strip" aria-label="Analiz ilerlemesi">
        <div className="progress-meta">
          <span>{progress.analyzedCustomers} / {progress.totalCustomers || 0}</span>
          <strong>{percentage}%</strong>
        </div>
        <div className="progress-track">
          <div className="progress-fill" style={{ width: `${percentage}%` }} />
        </div>
      </div>
    </section>
  );
}
