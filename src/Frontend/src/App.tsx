import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { useCallback, useEffect, useRef, useState } from "react";
import { AnalysisControls } from "./components/AnalysisControls";
import { LiveAnalysisTable } from "./components/LiveAnalysisTable";
import { RiskDistribution } from "./components/RiskDistribution";
import { SummaryCards } from "./components/SummaryCards";
import { analyzeAllCustomers, exportRiskyCustomers, getDashboardSummary, HUB_URL, seedCustomers } from "./api";
import type { AnalysisFailure, AnalysisProgress, DashboardSummary, LivePrediction } from "./types";
import { downloadCsv } from "./utils";

const initialProgress: AnalysisProgress = {
  isRunning: false,
  totalCustomers: 0,
  analyzedCustomers: 0,
  successCount: 0,
  failedCount: 0,
  connectionStatus: "connecting",
};

export default function App() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [livePredictions, setLivePredictions] = useState<LivePrediction[]>([]);
  const [failures, setFailures] = useState<AnalysisFailure[]>([]);
  const [progress, setProgress] = useState<AnalysisProgress>(initialProgress);
  const refreshCounter = useRef(0);

  const refreshDashboard = useCallback(async () => {
    const data = await getDashboardSummary();
    setSummary(data);
  }, []);

  useEffect(() => {
    refreshDashboard().catch((error) => {
      setProgress((current) => ({ ...current, lastMessage: error.message }));
    });
  }, [refreshDashboard]);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .build();

    connection.on("analysisStarted", (event: { totalCustomers: number }) => {
      refreshCounter.current = 0;
      setLivePredictions([]);
      setFailures([]);
      setProgress((current) => ({
        ...current,
        isRunning: true,
        totalCustomers: event.totalCustomers,
        analyzedCustomers: 0,
        successCount: 0,
        failedCount: 0,
        lastMessage: "Analiz başladı.",
      }));
    });

    connection.on("customerAnalyzed", (prediction: LivePrediction) => {
      refreshCounter.current += 1;
      setLivePredictions((current) => [prediction, ...current].slice(0, 200));
      setProgress((current) => ({
        ...current,
        isRunning: true,
        analyzedCustomers: prediction.analyzedCustomers ?? current.analyzedCustomers + 1,
        totalCustomers: prediction.totalCustomers ?? current.totalCustomers,
        successCount: current.successCount + 1,
        lastMessage: `Müşteri #${prediction.customerId} analiz edildi.`,
      }));

      if (refreshCounter.current % 10 === 0) {
        refreshDashboard().catch(() => undefined);
      }
    });

    connection.on("customerAnalysisFailed", (failure: AnalysisFailure) => {
      setFailures((current) => [failure, ...current].slice(0, 50));
      setProgress((current) => ({
        ...current,
        isRunning: true,
        analyzedCustomers: failure.analyzedCustomers,
        totalCustomers: failure.totalCustomers,
        failedCount: current.failedCount + 1,
        lastMessage: `Müşteri #${failure.customerId} analiz edilemedi.`,
      }));
    });

    connection.on("analysisCompleted", (event: { analyzedCustomers: number; totalCustomers: number }) => {
      setProgress((current) => ({
        ...current,
        isRunning: false,
        analyzedCustomers: event.totalCustomers,
        totalCustomers: event.totalCustomers,
        successCount: event.analyzedCustomers,
        lastMessage: "Analiz tamamlandı.",
      }));
      refreshDashboard().catch(() => undefined);
    });

    connection.onreconnecting(() => {
      setProgress((current) => ({ ...current, connectionStatus: "connecting", lastMessage: "Canlı bağlantı yeniden kuruluyor." }));
    });

    connection.onreconnected(() => {
      setProgress((current) => ({ ...current, connectionStatus: "connected", lastMessage: "Canlı bağlantı kuruldu." }));
    });

    connection.onclose(() => {
      setProgress((current) => ({ ...current, connectionStatus: "disconnected", lastMessage: "Canlı bağlantı kapandı." }));
    });

    connection
      .start()
      .then(() => {
        if (connection.state === HubConnectionState.Connected) {
          setProgress((current) => ({ ...current, connectionStatus: "connected" }));
        }
      })
      .catch((error) => {
        setProgress((current) => ({ ...current, connectionStatus: "disconnected", lastMessage: error.message }));
      });

    return () => {
      connection.stop().catch(() => undefined);
    };
  }, [refreshDashboard]);

  const handleAnalyze = async () => {
    try {
      setProgress((current) => ({ ...current, isRunning: true, lastMessage: "Analiz isteği gönderildi." }));
      await analyzeAllCustomers();
      await refreshDashboard();
      setProgress((current) => ({ ...current, isRunning: false, lastMessage: "Analiz tamamlandı." }));
    } catch (error) {
      setProgress((current) => ({
        ...current,
        isRunning: false,
        lastMessage: error instanceof Error ? error.message : "Analiz başlatılamadı.",
      }));
    }
  };

  const handleSeed = async () => {
    if (!window.confirm("Yeni seed verisi üretilecek. Devam edilsin mi?")) return;
    try {
      setProgress((current) => ({ ...current, lastMessage: "Seed verisi üretiliyor." }));
      await seedCustomers();
      await refreshDashboard();
      setProgress((current) => ({ ...current, lastMessage: "Seed verisi üretildi." }));
    } catch (error) {
      setProgress((current) => ({
        ...current,
        lastMessage: error instanceof Error ? error.message : "Seed işlemi başarısız.",
      }));
    }
  };

  const handleExport = async () => {
    try {
      const rows = await exportRiskyCustomers();
      downloadCsv(`riskli-musteriler-${new Date().toISOString().slice(0, 10)}.csv`, rows);
    } catch (error) {
      setProgress((current) => ({
        ...current,
        lastMessage: error instanceof Error ? error.message : "Rapor indirilemedi.",
      }));
    }
  };

  return (
    <main className="app-shell">
      <AnalysisControls
        progress={progress}
        onAnalyze={handleAnalyze}
        onSeed={handleSeed}
        onRefresh={() => refreshDashboard().catch(() => undefined)}
        onExport={handleExport}
      />

      <SummaryCards summary={summary} progress={progress} />

      <div className="dashboard-grid">
        <LiveAnalysisTable
          livePredictions={livePredictions}
          recentPredictions={summary?.recentPredictions ?? []}
        />
        <aside className="side-column">
          <RiskDistribution summary={summary} />
          <section className="panel failure-panel">
            <div className="panel-header">
              <h2>Hatalar</h2>
              <span>{failures.length}</span>
            </div>
            {failures.length === 0 ? (
              <p className="empty-text">Analiz hatası yok.</p>
            ) : failures.map((failure) => (
              <div className="failure-row" key={`${failure.customerId}-${failure.analyzedCustomers}`}>
                <strong>Müşteri #{failure.customerId}</strong>
                <span>{failure.message}</span>
              </div>
            ))}
          </section>
        </aside>
      </div>
    </main>
  );
}
