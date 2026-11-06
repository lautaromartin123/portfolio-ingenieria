"use client";
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  ArrowUpRight,
  ArrowDownRight,
  Target,
  ArrowRight,
  ShieldCheck,
  NotebookPen,
  Activity,
} from "lucide-react";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Table,
  TableHeader,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import {
  calc,
  dailyResults,
  dateKey,
  goalProgress,
  inPeriod,
  money,
  signed,
  stats,
  type JournalData,
  type Trade,
} from "@/lib/domain";
import { Button, Empty, Panel } from "./ui";
export function EquityChart({
  trades,
  timezone,
  initial = 0,
}: {
  trades: Trade[];
  timezone: string;
  initial?: number;
}) {
  let equity = initial;
  const points = [
    { date: "Inicio", equity },
    ...dailyResults(trades, timezone).map((d) => ({
      date: d.date.slice(5).replace("-", "/"),
      equity: (equity += d.pnl),
    })),
  ];
  return (
    <div
      className="equity-chart"
      role="img"
      aria-label={`Curva de rendimiento: resultado acumulado ${money(equity - initial)}`}
    >
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart
          data={points}
          margin={{ left: 0, right: 10, top: 20, bottom: 0 }}
        >
          <defs>
            <linearGradient id="equityGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#47c9a2" stopOpacity={0.22} />
              <stop offset="100%" stopColor="#47c9a2" stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid
            vertical={false}
            stroke="var(--border)"
            strokeDasharray="3 5"
          />
          <XAxis
            dataKey="date"
            axisLine={false}
            tickLine={false}
            minTickGap={45}
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
          />
          <YAxis
            axisLine={false}
            tickLine={false}
            width={65}
            tickFormatter={(n) => "$" + n.toLocaleString("en-US")}
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
          />
          <Tooltip
            contentStyle={{
              background: "var(--card)",
              border: "1px solid var(--border)",
              borderRadius: 10,
              color: "var(--foreground)",
            }}
            formatter={(v) => [money(Number(v)), "Acumulado"]}
          />
          <Area
            type="monotone"
            dataKey="equity"
            stroke="#47c9a2"
            strokeWidth={2.5}
            fill="url(#equityGradient)"
            isAnimationActive={false}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
export function RecentTrades({
  trades,
  onTrade,
}: {
  trades: Trade[];
  onTrade: (t: Trade) => void;
}) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Instrumento</TableHead>
          <TableHead>Dirección</TableHead>
          <TableHead>Setup</TableHead>
          <TableHead>Resultado</TableHead>
          <TableHead>R</TableHead>
          <TableHead>Plan</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {trades.map((t) => {
          const m = calc(t);
          return (
            <TableRow
              key={t.id}
              className="click-row"
              onClick={() => onTrade(t)}
            >
              <TableCell>
                <button
                  className="instrument"
                  onClick={(e) => {
                    e.stopPropagation();
                    onTrade(t);
                  }}
                >
                  <span className="instrument-avatar">
                    {t.instrument.slice(0, 2)}
                  </span>
                  <span>
                    <b>{t.instrument}</b>
                    <small>
                      {new Date(t.enteredAt).toLocaleDateString("es-AR", {
                        day: "2-digit",
                        month: "short",
                      })}
                    </small>
                  </span>
                </button>
              </TableCell>
              <TableCell>
                <span className="direction">
                  {t.direction === "Long" ? (
                    <ArrowUpRight size={14} />
                  ) : (
                    <ArrowDownRight size={14} />
                  )}{" "}
                  {t.direction}
                </span>
              </TableCell>
              <TableCell>{t.setup}</TableCell>
              <TableCell className={m.pnl >= 0 ? "positive" : "negative"}>
                <b>{signed(m.pnl)}</b>
              </TableCell>
              <TableCell>{m.r.toFixed(2)}R</TableCell>
              <TableCell>
                <span
                  className={`badge ${t.violations.length ? "warn" : "good"}`}
                >
                  {t.violations.length ? "Fuera del plan" : "Dentro del plan"}
                </span>
              </TableCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );
}
export default function Dashboard({
  data,
  period,
  setPeriod,
  onTrade,
  navigate,
  newTrade,
}: {
  data: JournalData;
  period: string;
  setPeriod: (p: string) => void;
  onTrade: (t: Trade) => void;
  navigate: (p: string) => void;
  newTrade: () => void;
}) {
  const ts = inPeriod(data.trades, period, data.plan.timezone),
    m = stats(ts),
    days = dailyResults(ts, data.plan.timezone),
    recent = [...ts]
      .sort((a, b) => Date.parse(b.enteredAt) - Date.parse(a.enteredAt))
      .slice(0, 5),
    compliant = ts.length
      ? (ts.filter((t) => !t.violations.length).length / ts.length) * 100
      : 0,
    visible = (data.settings.widgets as string[] | undefined) ?? [
      "equity",
      "calendar",
      "trades",
      "goals",
    ];
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">TU RENDIMIENTO, EN PERSPECTIVA</p>
          <h1>Dashboard</h1>
          <p>Buenas decisiones. Un trade a la vez.</p>
        </div>
        <Tabs value={period} onValueChange={setPeriod}>
          <TabsList className="period-tabs">
            {["Today", "Week", "Month", "3M", "Year", "All"].map((p) => (
              <TabsTrigger key={p} value={p}>
                {p}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      </div>
      <div className="summary-grid">
        <div className="summary-card featured">
          <span>
            P&L neto <span className="tiny-label">USD</span>
          </span>
          <strong className={m.pnl < 0 ? "negative" : ""}>
            {signed(m.pnl)}
          </strong>
          <div>
            <span className="tag">
              <Activity size={13} />
              {m.count} operaciones
            </span>
            <small>Comisiones incluidas</small>
          </div>
        </div>
        <div className="summary-card">
          <span>
            Win rate <Target size={16} />
          </span>
          <strong>
            {m.winRate.toFixed(1)}
            <em>%</em>
          </strong>
          <div className="win-bar">
            <i style={{ width: `${m.winRate}%` }} />
          </div>
          <small>
            <b className="positive">{m.wins} wins</b> ·{" "}
            <b className="negative">{m.losses} losses</b> · {m.be} BE
          </small>
        </div>
        <div className="summary-card">
          <span>
            Profit factor <ArrowUpRight size={16} />
          </span>
          <strong>
            {m.profitFactor === null ? "—" : m.profitFactor.toFixed(2)}
          </strong>
          <small>
            {m.profitFactor === null
              ? "Necesita al menos una pérdida"
              : "Ganancias / pérdidas netas"}
          </small>
        </div>
        <div className="summary-card">
          <span>
            Disciplina <ShieldCheck size={16} />
          </span>
          <strong>
            {compliant.toFixed(0)}
            <em>%</em>
          </strong>
          <small>Operaciones dentro del plan</small>
        </div>
      </div>
      <div className="mini-metrics">
        {[
          ["Total", signed(stats(data.trades).pnl)],
          [
            "Hoy",
            signed(
              stats(inPeriod(data.trades, "Today", data.plan.timezone)).pnl,
            ),
          ],
          [
            "Esta semana",
            signed(
              stats(inPeriod(data.trades, "Week", data.plan.timezone)).pnl,
            ),
          ],
          [
            "Este mes",
            signed(
              stats(inPeriod(data.trades, "Month", data.plan.timezone)).pnl,
            ),
          ],
          ["R/R promedio", m.rr.toFixed(2) + " : 1"],
          [
            "Racha actual",
            m.streak === 0
              ? "—"
              : Math.abs(m.streak) + (m.streak > 0 ? " wins" : " losses"),
          ],
        ].map(([k, v]) => (
          <div key={k}>
            <span>{k}</span>
            <b>{v}</b>
          </div>
        ))}
      </div>
      <div className="dashboard-middle">
        {visible.includes("equity") && (
          <Panel
            title="Curva de rendimiento"
            sub="P&L neto acumulado del período"
            action={
              <span className="chart-legend">
                <i />
                Equity
              </span>
            }
          >
            {ts.length ? (
              <EquityChart trades={ts} timezone={data.plan.timezone} />
            ) : (
              <Empty
                title="Tu proceso empieza acá"
                description="Registrá tu primera operación para ver la evolución de tu rendimiento."
                action={
                  <Button onClick={newTrade}>
                    Registrar un trade <ArrowUpRight size={16} />
                  </Button>
                }
              />
            )}
            <div className="chart-footer">
              <div>
                <span>Ganancia media</span>
                <b className="positive">{money(m.avgWin)}</b>
              </div>
              <div>
                <span>Pérdida media</span>
                <b className="negative">{money(m.avgLoss)}</b>
              </div>
              <div>
                <span>Mejor trade</span>
                <b>{money(m.best)}</b>
              </div>
              <div>
                <span>Peor trade</span>
                <b>{money(m.worst)}</b>
              </div>
            </div>
          </Panel>
        )}
        {visible.includes("goals") && (
          <Panel
            title="Tu enfoque"
            action={<Target size={18} className="muted" />}
          >
            <div className="focus-card">
              <span className="focus-icon">
                <ShieldCheck size={22} />
              </span>
              <h3>El proceso primero.</h3>
              <p>Tu ventaja también está en respetar tus propias reglas.</p>
              <Button variant="outline" onClick={() => navigate("plan")}>
                Ver mi plan <ArrowUpRight size={15} />
              </Button>
            </div>
            <div className="goals-mini">
              {data.goals
                .filter((g) => !g.archived)
                .slice(0, 2)
                .map((g) => {
                  const p = goalProgress(g, data);
                  return (
                    <div key={g.id}>
                      <div>
                        <span>{g.title}</span>
                        <b>
                          {p.count}/{g.target}
                        </b>
                      </div>
                      <Progress value={p.percent} />
                    </div>
                  );
                })}
              <button className="text-link" onClick={() => navigate("goals")}>
                {data.goals.length
                  ? "Ver objetivos"
                  : "Crear mi primer objetivo"}
                <ArrowRight size={15} />
              </button>
            </div>
          </Panel>
        )}
      </div>
      <div className="dashboard-bottom">
        {visible.includes("trades") && (
          <Panel
            title="Últimas operaciones"
            action={
              <button className="text-link" onClick={() => navigate("trades")}>
                Ver todas <ArrowRight size={14} />
              </button>
            }
          >
            {recent.length ? (
              <RecentTrades trades={recent} onTrade={onTrade} />
            ) : (
              <Empty
                title="Todavía no hay operaciones"
                description="Cada registro te ayuda a encontrar patrones."
              />
            )}
          </Panel>
        )}
        {visible.includes("calendar") && (
          <Panel
            title="Días de trading"
            sub={`${days.filter((d) => d.pnl > 0).length} positivos · ${days.filter((d) => d.pnl < 0).length} negativos`}
            action={
              <button
                className="icon-button"
                aria-label="Abrir calendario"
                onClick={() => navigate("calendar")}
              >
                <ArrowUpRight size={18} />
              </button>
            }
          >
            <MiniCalendar data={data} />
            <button
              className="journal-callout"
              onClick={() => navigate("daily")}
            >
              <NotebookPen size={18} />
              <span>
                Un momento para reflexionar
                <small>Completá tu Daily Journal</small>
              </span>
              <ArrowRight size={16} />
            </button>
          </Panel>
        )}
      </div>
    </>
  );
}
function MiniCalendar({ data }: { data: JournalData }) {
  const today = dateKey(new Date(), data.plan.timezone),
    first = today.slice(0, 7) + "-01",
    last = new Date(
      Number(today.slice(0, 4)),
      Number(today.slice(5, 7)),
      0,
    ).getDate(),
    offset = (new Date(first + "T12:00:00Z").getUTCDay() + 6) % 7,
    days = dailyResults(data.trades, data.plan.timezone);
  return (
    <div className="mini-calendar">
      <div className="calendar-month">
        {new Date(first + "T12:00:00Z").toLocaleDateString("es-AR", {
          month: "long",
          year: "numeric",
        })}
      </div>
      <div className="mini-days">
        {["L", "M", "M", "J", "V", "S", "D"].map((d, i) => (
          <small key={"h" + i}>{d}</small>
        ))}
        {Array.from({ length: offset }, (_, i) => (
          <span key={"e" + i} />
        ))}
        {Array.from({ length: last }, (_, i) => {
          const key = first.slice(0, 8) + String(i + 1).padStart(2, "0"),
            result = days.find((d) => d.date === key);
          return (
            <span
              key={key}
              title={result ? signed(result.pnl) : "Sin operaciones"}
              className={`${result ? (result.pnl > 0 ? "day-win" : result.pnl < 0 ? "day-loss" : "") : ""} ${key === today ? "is-today" : ""}`}
            >
              {i + 1}
            </span>
          );
        })}
      </div>
    </div>
  );
}
