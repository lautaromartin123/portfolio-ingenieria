import type { JournalData } from "./domain";
export async function api<T = unknown>(
  action: string,
  payload?: unknown,
): Promise<T> {
  const r = await fetch("/api/journal", {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Journal-Request": "1" },
    body: JSON.stringify({ action, payload }),
  });
  const result = (await r.json()) as T & { error?: string };
  if (!r.ok)
    throw new Error(result.error ?? "No se pudo guardar. Intentá nuevamente.");
  return result;
}
export async function fetchData() {
  const r = await fetch("/api/journal", { cache: "no-store" });
  const result = (await r.json()) as {
    error?: string;
    data: JournalData;
    user: { name: string; email: string };
  };
  if (!r.ok)
    throw new Error(result.error ?? "No se pudieron cargar los datos.");
  return result;
}
export async function uploadScreenshot(
  tradeId: string,
  kind: string,
  file: File,
) {
  const form = new FormData();
  form.set("tradeId", tradeId);
  form.set("kind", kind);
  form.set("file", file);
  const r = await fetch("/api/screenshots", {
    method: "POST",
    headers: { "X-Journal-Request": "1" },
    body: form,
  });
  const result = (await r.json()) as { error?: string; id: string };
  if (!r.ok) throw new Error(result.error ?? "La imagen no se pudo subir.");
  return result;
}
