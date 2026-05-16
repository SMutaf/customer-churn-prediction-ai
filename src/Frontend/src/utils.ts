import type { ModelExplanation, RiskLevel } from "./types";

export function formatPercent(value: number) {
  return `%${Math.round(value * 100)}`;
}

export function formatScore(value?: number) {
  if (value === undefined || Number.isNaN(value)) return "-";
  return value > 1 ? value.toFixed(1) : (value * 100).toFixed(1);
}

export function riskLabel(level: RiskLevel) {
  const labels: Record<string, string> = {
    Low: "Düşük",
    Medium: "Orta",
    High: "Yüksek",
    Critical: "Kritik",
  };

  return labels[level] ?? level;
}

export function parseExplanation(json?: string): ModelExplanation | null {
  if (!json) return null;
  try {
    return JSON.parse(json) as ModelExplanation;
  } catch {
    return null;
  }
}

export function downloadCsv(filename: string, rows: unknown[]) {
  const objects = rows.filter((row): row is Record<string, unknown> => typeof row === "object" && row !== null);
  if (objects.length === 0) return;

  const headers = Object.keys(objects[0]);
  const escape = (value: unknown) => `"${String(value ?? "").replace(/"/g, '""')}"`;
  const csv = [
    headers.join(";"),
    ...objects.map((row) => headers.map((header) => escape(row[header])).join(";")),
  ].join("\n");

  const blob = new Blob([`\uFEFF${csv}`], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
