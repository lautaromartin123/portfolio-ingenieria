"use client";
import { useState } from "react";
import {
  Plus,
  Download,
  Upload,
  Search,
  SlidersHorizontal,
  ArrowUpDown,
  Pencil,
  Trash2,
  Clock,
  ShieldCheck,
  TriangleAlert,
  ImagePlus,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import {
  Sheet,
  SheetContent,
  SheetTitle,
  SheetDescription,
} from "@/components/ui/sheet";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { toast } from "sonner";
import {
  calc,
  checkPlan,
  dateKey,
  emotions,
  money,
  signed,
  stats,
  tradeSchema,
  type JournalData,
  type Trade,
  type Psychology,
} from "@/lib/domain";
import { api, uploadScreenshot } from "@/lib/api";
import { downloadFile, exportCSV, importCSV } from "@/lib/csv";
import { Button, Choice, Check, Field, Note, Panel, Empty, Metric } from "./ui";
import { RecentTrades } from "./dashboard";
const localInput = (iso: string) => {
  const d = new Date(iso);
  return new Date(d.getTime() - d.getTimezoneOffset() * 60000)
    .toISOString()
    .slice(0, 16);
};
export function TradeForm({
  trade,
  data,
  onClose,
  onSaved,
  demo,
}: {
  trade: Trade;
  data: JournalData;
  onClose: () => void;
  onSaved: () => Promise<void>;
  demo: boolean;
}) {
  const [t, setT] = useState(trade),
    [busy, setBusy] = useState(false),
    [error, setError] = useState(""),
    [files, setFiles] = useState<{ kind: string; file: File }[]>([]);
  const valid = tradeSchema.safeParse(t),
    m = calc(t),
    violations = valid.success
      ? checkPlan(
          t,
          (data.trades.find((x) => x.id === t.id)
            ?.planSnapshot as JournalData["plan"]) ?? data.plan,
          data.trades,
        )
      : [];
  const update = <K extends keyof Trade>(k: K, v: Trade[K]) =>
    setT((p) => ({ ...p, [k]: v }));
  const psyche = <K extends keyof Psychology>(k: K, v: Psychology[K]) =>
    setT((p) => ({ ...p, psychology: { ...p.psychology, [k]: v } }));
  async function save(e: React.FormEvent) {
    e.preventDefault();
    if (demo) {
      toast.info("Salí de la demo para guardar tus propias operaciones.");
      return;
    }
    if (!valid.success) {
      setError(
        valid.error.issues
          .map((i) => i.path.join(".") + ": " + i.message)
          .join(" · "),
      );
      return;
    }
    setBusy(true);
    setError("");
    try {
      await api("saveTrade", valid.data);
      const remaining = [];
      for (const item of files) {
        try {
          await uploadScreenshot(t.id, item.kind, item.file);
        } catch {
          remaining.push(item);
        }
      }
      setFiles(remaining);
      const settings = {
        ...data.settings,
        widgets: data.settings.widgets ?? [
          "equity",
          "calendar",
          "trades",
          "goals",
        ],
        defaults: {
          instrument: t.instrument,
          pointValue: t.pointValue,
          quantity: t.quantity,
          commissions: t.commissions,
          setup: t.setup,
          timeframe: t.timeframe,
          session: t.session,
        },
      };
      await api("saveSettings", settings).catch(() => {});
      await onSaved();
      if (remaining.length) {
        setError(
          "Trade guardado. Algunas imágenes no se pudieron subir; podés reintentar.",
        );
      } else {
        toast.success("Operación guardada");
        onClose();
      }
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <Dialog open onOpenChange={(v) => !v && !busy && onClose()}>
      <DialogContent className="trade-modal">
        <DialogTitle>
          {data.trades.some((x) => x.id === t.id)
            ? "Editar operación"
            : "Nueva operación"}
        </DialogTitle>
        <DialogDescription>
          Registrá la ejecución. Aprendé del proceso.
        </DialogDescription>
        <form onSubmit={save}>
          <Tabs defaultValue="trade">
            <TabsList className="form-tabs">
              <TabsTrigger value="trade">Operación</TabsTrigger>
              <TabsTrigger value="mind">Psicología</TabsTrigger>
              <TabsTrigger value="notes">Notas y capturas</TabsTrigger>
            </TabsList>
            <TabsContent value="trade">
              <div className="form-grid three">
                <Field
                  label="Instrumento"
                  value={t.instrument}
                  onChange={(v) => {
                    update("instrument", v.toUpperCase());
                    const values: Record<string, number> = {
                      MES: 5,
                      MNQ: 2,
                      ES: 50,
                      NQ: 20,
                      CL: 1000,
                      MCL: 100,
                      GC: 100,
                      MGC: 10,
                    };
                    if (values[v.toUpperCase()])
                      update("pointValue", values[v.toUpperCase()]);
                  }}
                  required
                />
                <Choice
                  label="Dirección"
                  value={t.direction}
                  onChange={(v) => update("direction", v as Trade["direction"])}
                  options={["Long", "Short"]}
                />
                <Field
                  label="Contratos / cantidad"
                  value={t.quantity}
                  onChange={(v) =>
                    update("quantity", v === "" ? NaN : Number(v))
                  }
                  type="number"
                  step="any"
                  min={0}
                  required
                />
              </div>
              <div className="form-grid">
                {(["entry", "exit", "stop", "target"] as const).map(
                  (key, i) => (
                    <Field
                      key={key}
                      label={
                        [
                          "Precio de entrada",
                          "Precio de salida",
                          "Stop Loss inicial",
                          "Take Profit",
                        ][i]
                      }
                      value={t[key] === 0 ? "" : t[key]}
                      onChange={(v) => update(key, v === "" ? NaN : Number(v))}
                      type="number"
                      step="any"
                      min={0}
                      required
                    />
                  ),
                )}
              </div>
              <div className="form-grid">
                <Field
                  label="Entrada · fecha y hora"
                  type="datetime-local"
                  value={localInput(t.enteredAt)}
                  onChange={(v) =>
                    v && update("enteredAt", new Date(v).toISOString())
                  }
                  required
                />
                <Field
                  label="Salida · fecha y hora"
                  type="datetime-local"
                  value={localInput(t.exitedAt)}
                  onChange={(v) =>
                    v && update("exitedAt", new Date(v).toISOString())
                  }
                  required
                />
              </div>
              <p className="form-hint">
                Fechas en la zona de tu dispositivo:{" "}
                {Intl.DateTimeFormat().resolvedOptions().timeZone}. El plan se
                evalúa en {data.plan.timezone}.
              </p>
              <div className="form-grid">
                <Field
                  label="Valor por punto (USD)"
                  value={t.pointValue}
                  onChange={(v) =>
                    update("pointValue", v === "" ? NaN : Number(v))
                  }
                  type="number"
                  step="any"
                  min={0}
                  required
                />
                <Field
                  label="Comisiones totales (USD)"
                  value={t.commissions}
                  onChange={(v) =>
                    update("commissions", v === "" ? NaN : Number(v))
                  }
                  type="number"
                  step="any"
                  min={0}
                  required
                />
              </div>
              <p className="form-hint">
                Verificá el multiplicador del instrumento. Las comisiones
                incluyen entrada y salida.
              </p>
              <div className="form-grid three">
                <Field
                  label="Setup"
                  value={t.setup}
                  onChange={(v) => update("setup", v)}
                  required
                />
                <Choice
                  label="Timeframe"
                  value={t.timeframe}
                  onChange={(v) => update("timeframe", v)}
                  options={[
                    ...new Set([
                      "1m",
                      "3m",
                      "5m",
                      "15m",
                      "30m",
                      "1h",
                      "4h",
                      "1D",
                      t.timeframe,
                    ]),
                  ]}
                />
                <Choice
                  label="Sesión"
                  value={t.session}
                  onChange={(v) => update("session", v)}
                  options={[
                    ...new Set([
                      "New York",
                      "London",
                      "Asia",
                      "Otra",
                      t.session,
                    ]),
                  ]}
                />
              </div>
            </TabsContent>
            <TabsContent value="mind">
              <div className="form-grid">
                <Choice
                  label="Emoción antes"
                  value={t.psychology.before}
                  onChange={(v) => psyche("before", v)}
                  options={[
                    ...new Set([
                      ...emotions,
                      t.psychology.before,
                      t.psychology.after,
                    ]),
                  ]}
                />
                <Choice
                  label="Emoción después"
                  value={t.psychology.after}
                  onChange={(v) => psyche("after", v)}
                  options={emotions}
                />
              </div>
              <div className="form-grid three">
                {(["confidence", "quality", "adherence"] as const).map(
                  (key, i) => (
                    <Choice
                      key={key}
                      label={
                        [
                          "Confianza (1–5)",
                          "Calidad del setup (1–5)",
                          "Respeto del plan (1–5)",
                        ][i]
                      }
                      value={String(t.psychology[key])}
                      onChange={(v) => psyche(key, Number(v))}
                      options={["1", "2", "3", "4", "5"]}
                    />
                  ),
                )}
              </div>
              <div className="checks-grid">
                {(
                  [
                    "impulsive",
                    "movedStop",
                    "earlyClose",
                    "overtraded",
                    "fomo",
                  ] as const
                ).map((key, i) => (
                  <Check
                    key={key}
                    label={
                      [
                        "Fue impulsivo",
                        "Moví el Stop Loss",
                        "Cerré antes de tiempo",
                        "Sobreoperé",
                        "Sentí FOMO",
                      ][i]
                    }
                    value={t.psychology[key]}
                    onChange={(v) => psyche(key, v)}
                  />
                ))}
              </div>
              <Note
                label="Error cometido"
                value={t.psychology.error}
                onChange={(v) => psyche("error", v)}
                placeholder="Describí una decisión concreta…"
              />
              <Note
                label="Lección aprendida"
                value={t.psychology.lesson}
                onChange={(v) => psyche("lesson", v)}
                placeholder="¿Qué harías distinto la próxima vez?"
              />
            </TabsContent>
            <TabsContent value="notes">
              <Note
                label="Notas de la operación"
                value={t.notes}
                onChange={(v) => update("notes", v)}
                placeholder="Contexto, ejecución, observaciones…"
              />
              <div className="form-grid">
                {["before", "after"].map((kind) => (
                  <label className="upload-zone" key={kind}>
                    <ImagePlus size={24} />
                    <b>Captura {kind === "before" ? "antes" : "después"}</b>
                    <span>PNG, JPG o WebP · hasta 5 MB</span>
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/webp"
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (!file) return;
                        if (file.size > 5 * 1024 * 1024) {
                          toast.error("La imagen supera los 5 MB.");
                          return;
                        }
                        setFiles((f) => [
                          ...f.filter((x) => x.kind !== kind),
                          { kind, file },
                        ]);
                      }}
                    />
                    {files.find((f) => f.kind === kind)?.file.name}
                  </label>
                ))}
              </div>
            </TabsContent>
          </Tabs>
          <div className="trade-calculations">
            {[
              ["P&L neto", signed(m.pnl)],
              ["Riesgo USD", money(m.risk)],
              ["R obtenido", m.r.toFixed(2) + "R"],
              ["R/R", m.rr.toFixed(2) + " : 1"],
              ["Duración", m.duration.toFixed(0) + " min"],
            ].map(([k, v]) => (
              <div key={k}>
                <span>{k}</span>
                <b>{valid.success ? v : "—"}</b>
              </div>
            ))}
          </div>
          {valid.success && (
            <div
              className={`plan-check ${violations.length ? "has-violations" : ""}`}
            >
              {violations.length ? (
                <TriangleAlert size={18} />
              ) : (
                <ShieldCheck size={18} />
              )}
              <div>
                <b>
                  {violations.length
                    ? "Violación del plan"
                    : "Trade dentro del plan"}
                </b>
                {violations.length > 0 && (
                  <ul>
                    {violations.map((v) => (
                      <li key={v}>{v}</li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
          )}
          <p className="form-hint">
            Las reglas personales se evalúan con tu puntuación de respeto del
            plan. Podés registrar una violación para analizarla.
          </p>
          {error && (
            <div className="error-box" role="alert">
              {error}
            </div>
          )}
          <div className="form-footer">
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={busy}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={busy}>
              {busy ? "Guardando…" : "Guardar operación"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
export function TradeDetail({
  trade: t,
  data,
  onClose,
  onEdit,
  onDelete,
  refresh,
  confirm,
}: {
  trade: Trade;
  data: JournalData;
  onClose: () => void;
  onEdit: () => void;
  onDelete: () => void;
  refresh: () => Promise<void>;
  confirm: (title: string, action: () => Promise<void>) => void;
}) {
  const m = calc(t),
    shots = data.screenshots.filter((s) => s.tradeId === t.id);
  return (
    <Sheet open onOpenChange={(v) => !v && onClose()}>
      <SheetContent className="detail-sheet">
        <SheetTitle>
          {t.instrument} <span className="muted">{t.direction}</span>
        </SheetTitle>
        <SheetDescription>
          {new Date(t.enteredAt).toLocaleString("es-AR")}
        </SheetDescription>
        <div className="detail-pnl">
          <b className={m.pnl >= 0 ? "positive" : "negative"}>
            {signed(m.pnl)}
          </b>
          <span>
            {m.status} · {m.r.toFixed(2)}R
          </span>
        </div>
        <div className="button-row">
          <Button variant="outline" onClick={onEdit}>
            <Pencil size={15} />
            Editar
          </Button>
          <Button variant="outline" onClick={onDelete}>
            <Trash2 size={15} />
            Eliminar
          </Button>
        </div>
        <div className="detail-grid">
          {[
            ["Entrada", t.entry],
            ["Salida", t.exit],
            ["Stop inicial", t.stop],
            ["Take Profit", t.target],
            ["Cantidad", t.quantity],
            ["Valor / punto", money(t.pointValue)],
            ["Riesgo", money(m.risk)],
            ["Comisiones", money(t.commissions)],
            ["Setup", t.setup],
            ["Timeframe", t.timeframe],
            ["Sesión", t.session],
            ["Duración", m.duration.toFixed(0) + " min"],
          ].map(([k, v]) => (
            <div key={k}>
              <span>{k}</span>
              <b>{v}</b>
            </div>
          ))}
        </div>
        <div
          className={`plan-check ${t.violations.length ? "has-violations" : ""}`}
        >
          <ShieldCheck size={18} />
          <div>
            <b>
              {t.violations.length
                ? "Violación del plan"
                : "Trade dentro del plan"}
            </b>
            {t.violations.map((v) => (
              <p key={v}>{v}</p>
            ))}
          </div>
        </div>
        <h3>Psicología de la operación</h3>
        <div className="detail-grid">
          {[
            ["Antes", t.psychology.before],
            ["Después", t.psychology.after],
            ["Confianza", t.psychology.confidence + "/5"],
            ["Calidad", t.psychology.quality + "/5"],
            ["Respeto del plan", t.psychology.adherence + "/5"],
          ].map(([k, v]) => (
            <div key={k}>
              <span>{k}</span>
              <b>{v}</b>
            </div>
          ))}
        </div>
        <div className="button-row">
          {(
            [
              "impulsive",
              "movedStop",
              "earlyClose",
              "overtraded",
              "fomo",
            ] as const
          )
            .filter((k) => t.psychology[k])
            .map((k) => (
              <span className="badge warn" key={k}>
                {
                  {
                    impulsive: "Impulsivo",
                    movedStop: "Movió stop",
                    earlyClose: "Cierre anticipado",
                    overtraded: "Sobreoperó",
                    fomo: "FOMO",
                  }[k]
                }
              </span>
            ))}
        </div>
        {[
          ["Notas", t.notes],
          ["Error", t.psychology.error],
          ["Lección", t.psychology.lesson],
        ].map(([k, v]) => (
          <div className="detail-note" key={k}>
            <h3>{k}</h3>
            <p>{v || "Sin registrar"}</p>
          </div>
        ))}
        {shots.length > 0 && <h3>Capturas</h3>}
        {shots.map((s) => (
          <figure className="screenshot" key={s.id}>
            <a
              href={"/api/screenshots?id=" + s.id}
              target="_blank"
              rel="noreferrer"
            >
              <img
                src={"/api/screenshots?id=" + s.id}
                alt={`Captura ${s.kind === "before" ? "antes" : "después"} de ${t.instrument}`}
              />
            </a>
            <figcaption>
              {s.kind === "before" ? "Antes" : "Después"} · {s.name}
              <button
                aria-label="Eliminar captura"
                onClick={() =>
                  confirm("Eliminar esta captura", async () => {
                    const r = await fetch("/api/screenshots?id=" + s.id, {
                      method: "DELETE",
                      headers: { "X-Journal-Request": "1" },
                    });
                    if (!r.ok) throw new Error("No se pudo eliminar.");
                    await refresh();
                  })
                }
              >
                <Trash2 size={15} />
              </button>
            </figcaption>
          </figure>
        ))}
      </SheetContent>
    </Sheet>
  );
}
export default function Trades({
  data,
  onTrade,
  onNew,
  refresh,
  demo,
}: {
  data: JournalData;
  onTrade: (t: Trade) => void;
  onNew: () => void;
  refresh: () => Promise<void>;
  demo: boolean;
}) {
  const [query, setQuery] = useState(""),
    [instrument, setInstrument] = useState("all"),
    [setup, setSetup] = useState("all"),
    [result, setResult] = useState("all"),
    [direction, setDirection] = useState("all"),
    [plan, setPlan] = useState("all"),
    [from, setFrom] = useState(""),
    [to, setTo] = useState(""),
    [sort, setSort] = useState("newest"),
    [page, setPage] = useState(1),
    [imported, setImported] = useState<{
      trades: Trade[];
      errors: string[];
    } | null>(null),
    [busy, setBusy] = useState(false);
  const ts = data.trades
    .filter((t) => {
      const day = dateKey(t.exitedAt, data.plan.timezone);
      return (
        [t.instrument, t.setup, t.notes, t.psychology.error, t.session]
          .join(" ")
          .toLowerCase()
          .includes(query.toLowerCase()) &&
        (instrument === "all" || t.instrument === instrument) &&
        (setup === "all" || t.setup === setup) &&
        (result === "all" || calc(t).status === result) &&
        (direction === "all" || t.direction === direction) &&
        (plan === "all" || (plan === "yes") === !t.violations.length) &&
        (!from || day >= from) &&
        (!to || day <= to)
      );
    })
    .sort((a, b) =>
      sort === "highest"
        ? calc(b).pnl - calc(a).pnl
        : sort === "lowest"
          ? calc(a).pnl - calc(b).pnl
          : sort === "oldest"
            ? Date.parse(a.enteredAt) - Date.parse(b.enteredAt)
            : Date.parse(b.enteredAt) - Date.parse(a.enteredAt),
    );
  const pages = Math.max(1, Math.ceil(ts.length / 15)),
    current = Math.min(page, pages),
    m = stats(ts);
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">CADA OPERACIÓN CUENTA</p>
          <h1>Trading Journal</h1>
          <p>Tu historial de decisiones y resultados.</p>
        </div>
        <div className="button-row">
          <Button
            variant="outline"
            onClick={() =>
              downloadFile(
                exportCSV(ts),
                "trading-journal.csv",
                "text/csv;charset=utf-8",
              )
            }
          >
            <Download size={16} />
            Exportar
          </Button>
          <label className="file-button">
            <Upload size={16} />
            Importar
            <input
              type="file"
              accept=".csv,text/csv"
              onChange={async (e) => {
                const f = e.target.files?.[0];
                e.target.value = "";
                if (!f) return;
                try {
                  if (f.size > 5e6) throw new Error("El CSV supera los 5 MB.");
                  setImported(await importCSV(await f.text(), data.account.id));
                } catch (err) {
                  toast.error((err as Error).message);
                }
              }}
            />
          </label>
          <Button onClick={onNew}>
            <Plus size={16} />
            Nuevo trade
          </Button>
        </div>
      </div>
      <Panel>
        <div className="filters">
          <div className="search-field">
            <Search size={17} />
            <Input
              aria-label="Buscar operaciones"
              placeholder="Buscar instrumento, notas, setup…"
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
                setPage(1);
              }}
            />
          </div>
          <Choice
            label="Instrumento"
            value={instrument}
            onChange={setInstrument}
            options={[
              { value: "all", label: "Todos" },
              ...new Set(data.trades.map((t) => t.instrument)),
            ]}
          />
          <Choice
            label="Setup"
            value={setup}
            onChange={setSetup}
            options={[
              { value: "all", label: "Todos" },
              ...new Set(data.trades.map((t) => t.setup)),
            ]}
          />
          <Choice
            label="Resultado"
            value={result}
            onChange={setResult}
            options={[
              { value: "all", label: "Todos" },
              "Win",
              "Loss",
              "Break Even",
            ]}
          />
          <Choice
            label="Dirección"
            value={direction}
            onChange={setDirection}
            options={[{ value: "all", label: "Todas" }, "Long", "Short"]}
          />
          <Choice
            label="Plan"
            value={plan}
            onChange={setPlan}
            options={[
              { value: "all", label: "Todos" },
              { value: "yes", label: "Dentro del plan" },
              { value: "no", label: "Fuera del plan" },
            ]}
          />
          <Field label="Desde" type="date" value={from} onChange={setFrom} />
          <Field label="Hasta" type="date" value={to} onChange={setTo} />
          <Choice
            label="Ordenar"
            value={sort}
            onChange={setSort}
            options={[
              { value: "newest", label: "Más recientes" },
              { value: "oldest", label: "Más antiguas" },
              { value: "highest", label: "Mayor P&L" },
              { value: "lowest", label: "Menor P&L" },
            ]}
          />
        </div>
        <div className="results-bar">
          <span>{ts.length} operaciones</span>
          <b className={m.pnl >= 0 ? "positive" : "negative"}>
            {signed(m.pnl)}
          </b>
          <span>{m.winRate.toFixed(1)}% win rate</span>
          <button
            className="text-link"
            onClick={() => {
              setQuery("");
              setInstrument("all");
              setSetup("all");
              setResult("all");
              setDirection("all");
              setPlan("all");
              setFrom("");
              setTo("");
              setPage(1);
            }}
          >
            Limpiar filtros
          </button>
        </div>
        {ts.length ? (
          <RecentTrades
            trades={ts.slice((current - 1) * 15, current * 15)}
            onTrade={onTrade}
          />
        ) : (
          <Empty
            title="No hay operaciones para mostrar"
            description="Ajustá los filtros o registrá una nueva operación."
            action={<Button onClick={onNew}>Registrar operación</Button>}
          />
        )}
        <div className="pagination-bar">
          <span>
            Página {current} de {pages}
          </span>
          <Pagination>
            <PaginationContent>
              <PaginationItem>
                <PaginationPrevious
                  href="#"
                  aria-label="Página anterior"
                  onClick={(e) => {
                    e.preventDefault();
                    setPage(Math.max(1, current - 1));
                  }}
                />
              </PaginationItem>
              <PaginationItem>
                <PaginationNext
                  href="#"
                  aria-label="Página siguiente"
                  onClick={(e) => {
                    e.preventDefault();
                    setPage(Math.min(pages, current + 1));
                  }}
                />
              </PaginationItem>
            </PaginationContent>
          </Pagination>
        </div>
      </Panel>
      {imported && (
        <Dialog open onOpenChange={(v) => !v && !busy && setImported(null)}>
          <DialogContent className="import-modal">
            <DialogTitle>Revisar importación CSV</DialogTitle>
            <DialogDescription>
              {imported.trades.length} filas válidas · {imported.errors.length}{" "}
              errores. Los IDs ya existentes se omiten. Máximo 500 trades.
            </DialogDescription>
            {imported.errors.length > 0 && (
              <div className="error-box">
                {imported.errors.slice(0, 10).map((e, i) => (
                  <p key={i}>{e}</p>
                ))}
              </div>
            )}
            <div className="import-preview">
              <RecentTrades
                trades={imported.trades.slice(0, 5)}
                onTrade={() => {}}
              />
            </div>
            <p className="form-hint">
              Las reglas del plan actual se evaluarán al importar. Para una
              plantilla, exportá una operación. Los archivos sin ID usan una
              huella de sus campos para evitar duplicados.
            </p>
            <Button
              disabled={
                busy ||
                !imported.trades.length ||
                !!imported.errors.length ||
                demo
              }
              onClick={async () => {
                setBusy(true);
                try {
                  const r = await api<{ imported: number; skipped: number }>(
                    "importTrades",
                    imported.trades,
                  );
                  await refresh();
                  toast.success(
                    `${r.imported} importadas · ${r.skipped} duplicadas omitidas`,
                  );
                  setImported(null);
                } catch (e) {
                  toast.error((e as Error).message);
                } finally {
                  setBusy(false);
                }
              }}
            >
              {busy ? "Importando…" : "Confirmar importación"}
            </Button>
          </DialogContent>
        </Dialog>
      )}
    </>
  );
}
