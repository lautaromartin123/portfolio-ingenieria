import { tradeSchema, psychologySchema, type Trade } from "./domain";
const main = [
  "id",
  "enteredAt",
  "exitedAt",
  "instrument",
  "direction",
  "entry",
  "exit",
  "stop",
  "target",
  "quantity",
  "pointValue",
  "commissions",
  "setup",
  "timeframe",
  "session",
  "notes",
] as const;
const psych = [
  "before",
  "after",
  "confidence",
  "quality",
  "adherence",
  "impulsive",
  "movedStop",
  "earlyClose",
  "overtraded",
  "fomo",
  "error",
  "lesson",
] as const;
const columns = [...main, ...psych.map((k) => "psychology." + k)];
const escaped = (v: unknown) =>
  '"' + String(v ?? "").replaceAll('"', '""') + '"';
export function exportCSV(trades: Trade[]) {
  return (
    "\uFEFF" +
    [
      columns.join(","),
      ...trades.map((t) =>
        [...main.map((k) => t[k]), ...psych.map((k) => t.psychology[k])]
          .map((v) => {
            const s = String(v ?? "");
            return escaped(
              typeof v === "string" && /^[=+@'\t\r-]/.test(s) ? "'" + s : s,
            );
          })
          .join(","),
      ),
    ].join("\r\n")
  );
}
export function parseRows(text: string) {
  const rows: string[][] = [];
  let row: string[] = [],
    cell = "",
    quoted = false;
  const source = text.replace(/^\uFEFF/, "");
  for (let i = 0; i < source.length; i++) {
    const c = source[i];
    if (c === '"') {
      if (quoted && source[i + 1] === '"') {
        cell += '"';
        i++;
      } else quoted = !quoted;
    } else if (c === "," && !quoted) {
      row.push(cell);
      cell = "";
    } else if ((c === "\n" || c === "\r") && !quoted) {
      if (c === "\r" && source[i + 1] === "\n") i++;
      row.push(cell);
      if (row.some((x) => x.trim())) rows.push(row);
      row = [];
      cell = "";
    } else cell += c;
  }
  if (quoted) throw new Error("Hay comillas sin cerrar en el CSV.");
  row.push(cell);
  if (row.some((x) => x.trim())) rows.push(row);
  return rows;
}
export async function importCSV(text: string, accountId: string) {
  const rows = parseRows(text);
  if (rows.length < 2) throw new Error("El archivo no contiene operaciones.");
  if (rows.length > 501)
    throw new Error("Importá hasta 500 operaciones por archivo.");
  const header = rows.shift()!.map((s) => s.trim()),
    required = [
      "enteredAt",
      "exitedAt",
      "instrument",
      "direction",
      "entry",
      "exit",
      "stop",
      "target",
      "quantity",
      "pointValue",
      "commissions",
      "setup",
      "timeframe",
      "session",
    ];
  const missing = required.filter((c) => !header.includes(c));
  if (missing.length) throw new Error("Faltan columnas: " + missing.join(", "));
  const trades: Trade[] = [],
    errors: string[] = [];
  for (let i = 0; i < rows.length; i++) {
    try {
      if (rows[i].length !== header.length)
        throw new Error("La cantidad de columnas no coincide.");
      const r = Object.fromEntries(header.map((h, j) => [h, rows[i][j]]));
      for (const k of [
        "id",
        "notes",
        "setup",
        "session",
        "instrument",
        "timeframe",
        ...psych.map((k) => "psychology." + k),
      ])
        if (r[k]?.startsWith("'") && /^[=+@'\t\r-]/.test(r[k].slice(1)))
          r[k] = r[k].slice(1);
      const numeric = (key: string) => {
        if (!r[key]?.trim()) throw new Error(`${key}: valor vacío`);
        const v = Number(r[key]);
        if (!Number.isFinite(v)) throw new Error(`${key}: número inválido`);
        return v;
      };
      const p: Record<string, unknown> = {};
      for (const key of psych) {
        const raw = r["psychology." + key];
        if (raw === undefined || raw === "") continue;
        if (["confidence", "quality", "adherence"].includes(key))
          p[key] = Number(raw);
        else if (
          [
            "impulsive",
            "movedStop",
            "earlyClose",
            "overtraded",
            "fomo",
          ].includes(key)
        ) {
          if (!["true", "false"].includes(raw))
            throw new Error(`${key}: usá true o false`);
          p[key] = raw === "true";
        } else p[key] = raw;
      }
      let id = r.id;
      if (!id) {
        const hash = await crypto.subtle.digest(
          "SHA-256",
          new TextEncoder().encode(JSON.stringify(r)),
        );
        id =
          "csv-" +
          Array.from(new Uint8Array(hash))
            .map((b) => b.toString(16).padStart(2, "0"))
            .join("");
      }
      const t = tradeSchema.parse({
        ...r,
        id,
        accountId,
        entry: numeric("entry"),
        exit: numeric("exit"),
        stop: numeric("stop"),
        target: numeric("target"),
        quantity: numeric("quantity"),
        pointValue: numeric("pointValue"),
        commissions: numeric("commissions"),
        psychology: psychologySchema.parse(p),
        violations: [],
      });
      trades.push(t);
    } catch (e) {
      errors.push(
        `Fila ${i + 2}: ${e instanceof Error ? e.message : "Datos inválidos"}`,
      );
    }
  }
  return { trades, errors };
}
export function downloadFile(
  content: BlobPart,
  name: string,
  type = "application/json",
) {
  const url = URL.createObjectURL(new Blob([content], { type }));
  const a = document.createElement("a");
  a.href = url;
  a.download = name;
  a.click();
  setTimeout(() => URL.revokeObjectURL(url), 3000);
}
