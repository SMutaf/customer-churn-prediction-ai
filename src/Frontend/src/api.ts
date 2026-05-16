import type { DashboardSummary } from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5236/api";
export const HUB_URL = import.meta.env.VITE_SIGNALR_URL ?? "http://localhost:5236/hubs/analytics";

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
    ...options,
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export function getDashboardSummary() {
  return request<DashboardSummary>("/Reports/dashboard");
}

export function analyzeAllCustomers() {
  return request<{ message: string; analyzed_customers: number; date: string }>("/Analytics/analyze-all", {
    method: "POST",
  });
}

export function seedCustomers() {
  return request<string>("/Seed/generate-fake-data", {
    method: "POST",
  });
}

export function exportRiskyCustomers() {
  return request<unknown[]>("/Reports/export-risky-customers");
}
