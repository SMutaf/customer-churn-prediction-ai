export type RiskLevel = "Low" | "Medium" | "High" | "Critical" | string;

export interface RecentPrediction {
  customerId: number;
  customerName: string;
  churnScore: number;
  riskLevel: RiskLevel;
  recommendedAction: string;
  mainReason?: string;
}

export interface DashboardSummary {
  totalCustomers: number;
  highRiskCustomers: number;
  averageChurnScore: number;
  riskDistribution: Record<string, number>;
  recentPredictions: RecentPrediction[];
}

export interface ShapFactor {
  feature: string;
  value: number;
  shap_value: number;
  impact_direction: "increases_risk" | "decreases_risk" | string;
  explanation: string;
}

export interface ModelExplanation {
  method?: string;
  base_value?: number;
  prediction_value?: number;
  top_positive_factors?: ShapFactor[];
  top_negative_factors?: ShapFactor[];
  error?: string;
}

export interface LivePrediction {
  id?: number;
  customerId: number;
  churnScore: number;
  coreRiskScore?: number;
  mlChurnProbability?: number;
  finalRiskScore?: number;
  riskLevel: RiskLevel;
  segment?: string;
  recommendedAction: string;
  mainReason?: string;
  modelExplanationsJson?: string;
  predictionDate?: string;
  analyzedCustomers?: number;
  totalCustomers?: number;
}

export interface AnalysisProgress {
  isRunning: boolean;
  totalCustomers: number;
  analyzedCustomers: number;
  successCount: number;
  failedCount: number;
  connectionStatus: "connecting" | "connected" | "disconnected";
  lastMessage?: string;
}

export interface AnalysisFailure {
  customerId: number;
  message: string;
  analyzedCustomers: number;
  totalCustomers: number;
}
