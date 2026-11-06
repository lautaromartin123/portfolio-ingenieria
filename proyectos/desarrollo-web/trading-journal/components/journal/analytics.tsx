"use client";
import { useState } from "react";
import {
  BarChart,
  Bar,
  Cell,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { TrendingUp, Brain, ShieldCheck, TriangleAlert } from "lucide-react";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  calc,
  dateKey,
  inPeriod,
  money,
  signed,
  stats,
  type Trade,
  type JournalData,
} from "@/lib/domain";
import { Choice, Panel, Metric, Empty } from "./ui";
import { EquityChart } from "./dashboard";
export function groupTrades(trades: Trade[], key: string, timezone: string) {
  const groups: Record<string, Trade[]> = {};
  for (const t of trades) {
    let name = "";
    if (
      key === "instrument" ||
      key === "setup" ||
      key === "direction" ||
      key === "session"
    )
      name = t[key];
    if (key === "weekday")
      name = new Intl.DateTimeFormat("es-AR", {
        timeZone: timezone,
        weekday: "long",
      }).format(new Date(t.enteredAt));
    if (key === "hour")
      name =
        new Intl.DateTimeFormat("en-GB", {
          timeZone: timezone,
          hour: "2-digit",
          hourCycle: "h23",
        }).format(new Date(t.enteredAt)) + ":00";
    if (key === "month") name = dateKey(t.exitedAt, timezone).slice(0, 7);
    if (key === "confidence") name = t.psychology.confidence + "/5";
    if (key === "quality") name = t.psychology.quality + "/5";
    (groups[name] ??= []).push(t);
  }
  return Object.entries(groups)
    .map(([name, ts]) => ({ name, ...stats(ts) }))
    .sort((a, b) => b.pnl - a.pnl);
}
export function ResultsBars({
  rows,
}: {
  rows: { name: string; pnl: number }[];
}) {
  return (
    <div
      className="bar-chart"
      role="img"
      aria-label={rows.map((r) => r.name + ": " + money(r.pnl)).join(", ")}
    >
      <ResponsiveContainer width="100%" height="100%">
        <BarChart
          data={rows.slice(0, 15)}
          layout="vertical"
          margin={{ left: 6, right: 28, top: 15, bottom: 12 }}
        >
          <CartesianGrid
            horizontal={false}
            stroke="var(--border)"
            strokeDasharray="3 5"
          />
          <XAxis
            type="number"
            axisLine={false}
            tickLine={false}
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
            tickFormatter={(n) => "$" + n}
          />
          <YAxis
            type="category"
            dataKey="name"
            width={105}
            tickLine={false}
            axisLine={false}
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
          />
          <Tooltip
            contentStyle={{
              background: "var(--card)",
              border: "1px solid var(--border)",
              borderRadius: 8,
            }}
            formatter={(v) => [money(Number(v)), "P&L"]}
          />
          <Bar
            dataKey="pnl"
            maxBarSize={24}
            radius={[0, 4, 4, 0]}
            isAnimationActive={false}
          >
            {rows.slice(0, 15).map((r, i) => (
              <Cell
                key={i}
                fill={r.pnl >= 0 ? "var(--positive)" : "var(--negative)"}
              />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
export default function Analytics({ data }: { data: JournalData }) {
  const [period, setPeriod] = useState("All"),
    [group, setGroup] = useState("setup");
  const trades = inPeriod(data.trades, period, data.plan.timezone),
    rows = groupTrades(trades, group, data.plan.timezone),
    impulsive = stats(trades.filter((t) => t.psychology.impulsive)),
    respect = stats(trades.filter((t) => !t.violations.length)),
    outside = stats(trades.filter((t) => t.violations.length)),
    errors: Record<string, number> = {};
  for (const t of trades) {
    if (t.psychology.error.trim())
      errors[t.psychology.error.trim()] =
        (errors[t.psychology.error.trim()] ?? 0) + 1;
    for (const [k, label] of [
      ["impulsive", "Impulsividad"],
      ["fomo", "FOMO"],
      ["movedStop", "Mover el stop"],
      ["earlyClose", "Cierre anticipado"],
      ["overtraded", "Sobreoperar"],
    ] as const)
      if (t.psychology[k]) errors[label] = (errors[label] ?? 0) + 1;
  }
  const hours = groupTrades(trades, "hour", data.plan.timezone),
    setups = groupTrades(trades, "setup", data.plan.timezone);
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">ENCONTRÁ TU VENTAJA</p>
          <h1>Analytics</h1>
          <p>Patrones de rendimiento y comportamiento.</p>
        </div>
        <Tabs value={period} onValueChange={setPeriod}>
          <TabsList className="period-tabs">
            {["Today", "Week", "Month", "3M", "Year", "All"].map((p) => (
              <TabsTrigger value={p} key={p}>
                {p}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      </div>
      <div className="stat-strip">
        <Metric
          label="Mejor horario"
          value={hours[0]?.name ?? "—"}
          hint={
            hours[0]
              ? signed(hours[0].pnl) + " · " + hours[0].count + " trades"
              : "Sin operaciones"
          }
        />
        <Metric
          label="Setup más rentable"
          value={setups[0]?.name ?? "—"}
          hint={setups[0] ? signed(setups[0].pnl) : "Sin operaciones"}
        />
        <Metric
          label="Trades impulsivos"
          value={signed(impulsive.pnl)}
          positive={impulsive.pnl >= 0}
          hint={`${impulsive.count} trades · pérdidas ${money(trades.filter((t) => t.psychology.impulsive && calc(t).pnl < 0).reduce((s, t) => s + calc(t).pnl, 0))}`}
        />
        <Metric
          label="Resultado promedio en R"
          value={stats(trades).r.toFixed(2) + "R"}
        />
      </div>
      <Panel
        title="Rendimiento por categoría"
        sub="Ordenado por P&L neto. Compará también el tamaño de la muestra."
        action={
          <Choice
            label="Agrupar por"
            value={group}
            onChange={setGroup}
            options={[
              { value: "instrument", label: "Instrumento" },
              { value: "setup", label: "Setup" },
              { value: "direction", label: "Long vs Short" },
              { value: "weekday", label: "Día de la semana" },
              { value: "hour", label: "Hora de entrada" },
              { value: "session", label: "Sesión" },
              { value: "month", label: "Mes" },
              { value: "confidence", label: "Confianza" },
              { value: "quality", label: "Calidad del setup" },
            ]}
          />
        }
      >
        {rows.length ? (
          <>
            <ResultsBars rows={rows} />
            <Table>
              <TableHeader>
                <TableRow>
                  {[
                    "Categoría",
                    "Trades",
                    "Win rate",
                    "P&L neto",
                    "R medio",
                    "Profit factor",
                  ].map((h) => (
                    <TableHead key={h}>{h}</TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((r) => (
                  <TableRow key={r.name}>
                    <TableCell>{r.name}</TableCell>
                    <TableCell>{r.count}</TableCell>
                    <TableCell>{r.winRate.toFixed(1)}%</TableCell>
                    <TableCell className={r.pnl >= 0 ? "positive" : "negative"}>
                      {signed(r.pnl)}
                    </TableCell>
                    <TableCell>{r.r.toFixed(2)}R</TableCell>
                    <TableCell>{r.profitFactor?.toFixed(2) ?? "—"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </>
        ) : (
          <Empty
            title="Todavía no hay patrones para analizar"
            description="Los gráficos se construyen con tus operaciones cerradas."
          />
        )}
      </Panel>
      <div className="two-columns section-gap">
        <Panel
          title="Respetar el plan"
          sub="Resultado de la evaluación registrada en cada operación."
        >
          <ResultsBars
            rows={[
              { name: "Dentro del plan", pnl: respect.pnl },
              { name: "Fuera del plan", pnl: outside.pnl },
            ]}
          />
          <div className="chart-footer">
            <div>
              <span>Dentro del plan</span>
              <b>{respect.count} trades</b>
            </div>
            <div>
              <span>Win rate</span>
              <b>{respect.winRate.toFixed(1)}%</b>
            </div>
            <div>
              <span>Fuera del plan</span>
              <b>{outside.count} trades</b>
            </div>
            <div>
              <span>Win rate</span>
              <b>{outside.winRate.toFixed(1)}%</b>
            </div>
          </div>
        </Panel>
        <Panel
          title="Errores más frecuentes"
          sub="Un trade puede tener más de un error."
        >
          <div className="error-ranking">
            {Object.entries(errors)
              .sort((a, b) => b[1] - a[1])
              .slice(0, 8)
              .map(([name, count], i) => (
                <div key={name}>
                  <span className="rank">{i + 1}</span>
                  <span>{name}</span>
                  <b>{count}</b>
                </div>
              ))}
            {!Object.keys(errors).length && (
              <Empty
                title="Sin errores registrados"
                description="La evaluación psicológica te ayuda a detectarlos."
              />
            )}
          </div>
        </Panel>
      </div>
      <Panel
        title="Rendimiento acumulado"
        sub={`Todos los importes en USD · Zona horaria: ${data.plan.timezone}`}
      >
        <EquityChart trades={trades} timezone={data.plan.timezone} />
      </Panel>
    </>
  );
}
